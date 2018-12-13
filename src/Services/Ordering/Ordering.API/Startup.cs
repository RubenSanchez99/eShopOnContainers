using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventFlow;
using EventFlow.Extensions;
using EventFlow.Autofac.Extensions;
using EventFlow.Configuration;
using EventFlow.MetadataProviders;
using EventFlow.TestHelpers;
using EventFlow.AspNetCore.MetadataProviders;
using EventFlow.AspNetCore.Extensions;
using EventFlow.Aspnetcore.Middlewares;
using EventFlow.EventStores.EventStore;
using EventFlow.EventStores.EventStore.Extensions;
using Ordering.API.Services;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using Microsoft.Extensions.Hosting;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Ordering.ReadModel;
using Ordering.API.Application.Services;
using Ordering.API.Application.IntegrationEvents.EventHandling;
using Ordering.Domain.Events;
using Ordering.API.Application.Sagas;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Infrastructure;
using GreenPipes;
using MassTransit.Saga;
using Ordering.API.Application.Subscribers;

namespace Ordering.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        private ILoggerFactory _loggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var containerBuilder = new ContainerBuilder();

            services.AddScoped<IHostedService, MassTransitHostedService>();
            services.AddScoped<UserCheckoutAcceptedIntegrationEventHandler>();
            services.AddScoped<OrderStockConfirmedIntegrationEventHandler>();
            services.AddScoped<OrderStockRejectedIntegrationEventHandler>();
            services.AddScoped<OrderPaymentSuccededIntegrationEventHandler>();
            services.AddScoped<OrderPaymentFailedIntegrationEventHandler>();
            services.AddScoped<GracePeriodConfirmedIntegrationEventHandler>();

            var eventStoreUri = new Uri(Environment.GetEnvironmentVariable("EVENTSTORE_URL"));
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");

            var connectionSettings = ConnectionSettings.Create()
                .EnableVerboseLogging()
                .KeepReconnecting()
                .KeepRetrying()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .Build();

            var events = new List<Type>() {
                typeof(OrderStartedDomainEvent),
                typeof(BuyerCreatedDomainEvent),
                typeof(BuyerPaymentMethodAddedDomainEvent),
                typeof(BuyerAndPaymentMethodVerifiedDomainEvent),
                typeof(OrderStatusChangedToAwaitingValidationDomainEvent),
                typeof(OrderStatusChangedToPaidDomainEvent),
                typeof(OrderStatusChangedToStockConfirmedDomainEvent),
                typeof(OrderBuyerChangedDomainEvent),
                typeof(OrderPaymentMethodChangedDomainEvent)
            };

            var container = EventFlowOptions.New
                .UseAutofacContainerBuilder(containerBuilder)
                .UseConsoleLog()
                //.AddAspNetCoreMetadataProviders()
                //.AddEvents(typeof(OrderStartedDomainEvent).Assembly)
                .AddEvents(events)
                .AddCommandHandlers(typeof(Startup).Assembly)
                .AddSubscribers(typeof(Startup).Assembly)
                .AddQueryHandlers(typeof(OrderingDbContext).Assembly)
                //.AddDefaults(typeof(Startup).Assembly)
                .UseEventStoreEventStore(eventStoreUri, connectionSettings)
                .AddEntityFrameworkReadModel();
                
            /*
            var options = new DbContextOptionsBuilder<SagaDbContext<GracePeriod, SagaDbContextMapping>>()
                .UseSqlServer(@"Server=sql.data;Initial Catalog=CapacitacionMicroservicios.OrderingDb;User Id=sa;Password=Pass@word")
                .Options;

            Func<DbContext> contextFactory = () => 
                new SagaDbContext<GracePeriod, SagaDbContextMapping>(options);

            var repository = new EntityFrameworkSagaRepository<GracePeriod>(
                contextFactory, optimistic: false);

            */

            containerBuilder.Register(c =>
            {
                var busControl = Bus.Factory.CreateUsingRabbitMq(sbc => 
                {
                    var host = sbc.Host(new Uri("rabbitmq://rabbitmq"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    sbc.ReceiveEndpoint(host, "basket_checkout_queue", e => 
                    {
                        e.Consumer<UserCheckoutAcceptedIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "stock_confirmed_queue", e => 
                    {
                        e.Consumer<OrderStockConfirmedIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "stock_rejected_queue", e => 
                    {
                        e.Consumer<OrderStockRejectedIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "payment_succeded_queue", e => 
                    {
                        e.Consumer<OrderPaymentSuccededIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "payment_failed_queue", e => 
                    {
                        e.Consumer<OrderPaymentFailedIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "graceperiod_confirmed_queue", e => 
                    {
                        e.Consumer<GracePeriodConfirmedIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "order_validation_state", e =>
                    {
                        e.UseRetry(x => 
                            {
                                x.Handle<DbUpdateConcurrencyException>();
                                x.Interval(5, TimeSpan.FromMilliseconds(100));
                            }); // Add the retry middleware for optimistic concurrency
                        e.StateMachineSaga(new GracePeriodStateMachine(), new InMemorySagaRepository<GracePeriod>());
                    });
                    sbc.UseExtensionsLogging(_loggerFactory);
                    sbc.UseInMemoryScheduler();
                });
                var consumeObserver = new ConsumeObserver(_loggerFactory.CreateLogger<ConsumeObserver>());
                busControl.ConnectConsumeObserver(consumeObserver);

                var sendObserver = new SendObserver(_loggerFactory.CreateLogger<SendObserver>());
                busControl.ConnectSendObserver(sendObserver);

                return busControl;
            })
            .As<IBusControl>()
            .As<IPublishEndpoint>()
            .SingleInstance();

            services.AddTransient<IOrderingService, OrderingService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            containerBuilder.Populate(services);

            return new AutofacServiceProvider(containerBuilder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<CommandPublishMiddleware>();
            app.UseMvc();
        }
    }
}

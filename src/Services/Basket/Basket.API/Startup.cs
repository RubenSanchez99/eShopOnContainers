using Basket.API.Infrastructure.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Basket.API.IntegrationEvents.EventHandling;
using Basket.API.Model;
using Basket.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

// Para MassTransit
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.AutofacIntegration;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Basket.API
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
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                options.Filters.Add(typeof(ValidateModelStateFilter));

            }).AddControllersAsServices();

            ConfigureAuthService(services);

            services.Configure<BasketSettings>(Configuration);           

            //By connecting here we are making sure that our service
            //cannot start until redis is ready. This might slow down startup,
            //but given that there is a delay on resolving the ip address
            //and then creating the connection it seems reasonable to move
            //that cost to startup instead of having the first request pay the
            //penalty.
            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;
                var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);           
                
                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Basket HTTP API",
                    Version = "v1",
                    Description = "The Basket Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = $"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize",
                    TokenUrl = $"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token",
                    Scopes = new Dictionary<string, string>()
                    {
                        { "basket", "Basket API" }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    policyBuilder => policyBuilder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IBasketRepository, RedisBasketRepository>();
            services.AddTransient<IIdentityService, IdentityService>();
            
            services.AddOptions();

            // Configurar MassTransit
            services.AddScoped<IHostedService, MassTransitHostedService>();
            services.AddScoped<ProductPriceChangedIntegrationEventHandler>();
            services.AddScoped<OrderStartedIntegrationEventHandler>();

            var builder = new ContainerBuilder(); 

            builder.Register(c =>
            {
                var busControl = Bus.Factory.CreateUsingRabbitMq(sbc => 
                {
                    var host = sbc.Host(new Uri("rabbitmq://rabbitmq"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    sbc.ReceiveEndpoint(host, "price_updated_queue", e => 
                    {
                        e.Consumer<ProductPriceChangedIntegrationEventHandler>(c);
                    });
                    sbc.ReceiveEndpoint(host, "order_started_queue", e => 
                    {
                        e.Consumer<OrderStartedIntegrationEventHandler>(c);
                    });
                    sbc.UseExtensionsLogging(_loggerFactory);
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
            builder.Populate(services);
            var container = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(container);
        }

      

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }            
            app.UseStaticFiles();          
            app.UseCors("CorsPolicy");

            ConfigureAuth(app);

            app.UseMvcWithDefaultRoute();

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                   c.ConfigureOAuth2("basketswaggerui", "", "", "Basket Swagger UI");
               });

        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var identityUrl = Configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = "http://identity.api"; // ¿Aquí debe ser localhost o identity.api?
                options.MetadataAddress = "http://identity.api/.well-known/openid-configuration";
                options.RequireHttpsMetadata = false; // Busca como cambiar el issuer en identity.api
                options.Audience = "http://identity.api/resources";
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidAudiences = new List<string> 
                    {
                        "postman",
                    }
                };

            });
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app)
        {
            app.UseAuthentication();
        }
    }
}

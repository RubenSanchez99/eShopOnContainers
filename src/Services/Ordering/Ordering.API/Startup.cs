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
using Ordering.API.Services;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Hosting;

namespace Ordering.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var containerBuilder = new ContainerBuilder();

            var container = EventFlowOptions.New
                .UseAutofacContainerBuilder(containerBuilder)
                .AddDefaults(typeof(Startup).Assembly)
                //.AddEntityFrameworkReadModel()
                .AddAspNetCoreMetadataProviders();

            containerBuilder.Register(c =>
            {
                return Bus.Factory.CreateUsingRabbitMq(sbc => 
                    sbc.Host(new Uri("rabbitmq://rabbitmq"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    })
                );
            })
            .As<IBusControl>()
            .As<IPublishEndpoint>()
            .SingleInstance();

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

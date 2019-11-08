using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Infrastructure;
using Weikio.EventFramework.Configuration;
using Weikio.EventFramework.EventAggregator;
using Weikio.EventFramework.Extensions;
using Weikio.EventFramework.Publisher;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventFramework(this IServiceCollection services, Action<EventFrameworkOptions> setupAction = null)
        {
            var builder = new EventFrameworkBuilder(services);

            builder.Services.TryAddSingleton<ICloudEventPublisher, CloudEventPublisher>();
            builder.Services.TryAddSingleton<ICloudEventGatewayCollection, CloudEventGatewayCollection>();
            builder.Services.TryAddSingleton<ICloudEventAggregator, CloudEventAggregator>();
            builder.Services.TryAddSingleton<ICloudEventRouterServiceFactory, CloudEventRouterServiceFactory>();
            builder.Services.TryAddTransient<ICloudEventRouterService, CloudEventRouterService>();
            builder.Services.TryAddSingleton<ICloudEventRouteCollection, CloudEventRouteCollection>();
            builder.Services.AddStartupTasks();
            
            services.AddSingleton<HttpGatewayChangeToken>();
            services.AddSingleton<IActionDescriptorChangeProvider, HttpGatewayActionDescriptorChangeProvider>();
            services.AddSingleton<HttpGatewayChangeNotifier>();

            services.AddHttpContextAccessor();

            
            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }
            
            return builder;
        }
    }
}

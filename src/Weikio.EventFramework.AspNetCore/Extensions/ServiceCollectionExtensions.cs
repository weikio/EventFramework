using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventAggregator.AspNetCore;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Extensions;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventFramework(this IServiceCollection services, Action<EventFrameworkOptions> setupAction = null)
        {
            var builder = new EventFrameworkBuilder(services);

            builder.AddCloudEventPublisher();
            builder.AddCloudEventCreator();
            builder.AddCloudEventAggregator();
            builder.AddCloudEventGateway();
            builder.AddCloudEventSources();

            builder.Services.TryAddSingleton<ICloudEventRouteCollection, CloudEventRouteCollection>();

            services.AddHostedService<ServiceCreationHostedService>();
            services.AddHostedService<RouteInitializationStartupTask>();
            
            builder.Services.TryAddSingleton<RouteInitializer>();

            var options = new EventFrameworkOptions();
            setupAction?.Invoke(options);
            
            var conf = Options.Create(options);
            builder.Services.AddSingleton<IOptions<EventFrameworkOptions>>(conf);

            return builder;
        }
    }

    public class EventFrameworkOptions
    {
    }

}

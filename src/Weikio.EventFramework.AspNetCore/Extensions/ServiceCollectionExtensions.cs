using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.AspNetCore;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Extensions;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
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
            builder.AddCloudEventDataflowChannels();
            builder.AddCloudEventIntegrationFlows();

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

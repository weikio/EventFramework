using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventIntegrationFlows(this IEventFrameworkBuilder builder)
        {
            var services = builder.Services;

            services.AddCloudEventIntegrationFlows();
            
            return builder;
        }
        
        public static IServiceCollection AddCloudEventIntegrationFlows(this IServiceCollection services)
        {
            services.AddSingleton<DefaultCloudEventFlowManager>();
            services.AddHostedService<EventFlowProviderStartupHandler>();
            services.AddHostedService<EventFlowStartupService>();
            services.TryAddSingleton<ICloudEventFlowManager, DefaultCloudEventFlowManager>();
            services.TryAddSingleton<EventFlowProvider>();
            services.TryAddSingleton<IEventFlowInstanceFactory, DefaultEventFlowInstanceFactory>();
            services.TryAddSingleton<EventFlowDefinitionProvider>();
            
            return services;
        }
    }
}

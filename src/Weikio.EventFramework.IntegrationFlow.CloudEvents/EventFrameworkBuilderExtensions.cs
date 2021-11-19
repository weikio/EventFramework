using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
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
            services.AddSingleton<DefaultCloudEventsIntegrationFlowManager>();
            services.AddHostedService<IntegrationFlowProviderStartupHandler>();
            services.AddHostedService<IntegrationFlowStartupService>();
            services.TryAddSingleton<ICloudEventsIntegrationFlowManager, DefaultCloudEventsIntegrationFlowManager>();
            services.TryAddSingleton<IntegrationFlowProvider>();
            services.TryAddSingleton<IIntegrationFlowInstanceFactory, DefaultIntegrationFlowInstanceFactory>();
            
            return services;
        }
    }
}

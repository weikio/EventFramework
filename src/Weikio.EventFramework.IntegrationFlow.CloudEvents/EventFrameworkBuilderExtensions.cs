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

            services.AddSingleton<DefaultCloudEventsIntegrationFlowManager>();
            services.AddHostedService<IntegrationFlowStartupService>();
            services.TryAddSingleton<ICloudEventsIntegrationFlowManager, DefaultCloudEventsIntegrationFlowManager>();
            
            return builder;
        }
    }
}

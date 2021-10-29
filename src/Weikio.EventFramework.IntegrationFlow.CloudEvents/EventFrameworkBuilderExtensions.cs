using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public static class EventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventIntegrationFlows(this IEventFrameworkBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<CloudEventsIntegrationFlowManager>();
            
            return builder;
        }
    }
}
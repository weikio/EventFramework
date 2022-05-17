using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.AzureServiceBus
{
    public class AzureServiceBus : IComponentBuilder
    {
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBus(AzureServiceBusConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<AzureServiceBusComponent>>();

            var component = new AzureServiceBusComponent(_configuration, logger);

            return Task.FromResult<CloudEventsComponent>(component);
        }
    }
}

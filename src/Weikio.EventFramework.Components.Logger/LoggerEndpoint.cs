using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Logger
{
    public class LoggerEndpoint : IComponentBuilder
    {
        public LoggerEndpoint(LoggerEndpointOptions configuration = null)
        {
            Configuration = configuration ?? new LoggerEndpointOptions();
        }

        public LoggerEndpointOptions Configuration { get; set; }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<LoggerEndpointComponent>>();

            var result = new LoggerEndpointComponent(Configuration, logger);

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }
}

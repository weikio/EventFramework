using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.File
{
    public class FileEndpoint : IComponentBuilder
    {
        public FileEndpoint(FileEndpointOptions configuration = null)
        {
            Configuration = configuration ?? new FileEndpointOptions();
        }

        public FileEndpointOptions Configuration { get; set; }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<FileEndpointComponent>>();

            var result = new FileEndpointComponent(Configuration, logger);

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }
}

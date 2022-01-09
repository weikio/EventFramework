using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Security
{
    public class DecryptorComponentBuilder : IComponentBuilder
    {
        public DecryptorComponentBuilder(DecryptorComponentOptions configuration = null)
        {
            Configuration = configuration ?? new DecryptorComponentOptions();
        }

        public DecryptorComponentOptions Configuration { get; set; }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var result = new DecryptorComponent(Configuration, context.ServiceProvider.GetRequiredService<IChannelManager>());

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }
}
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Security
{
    public class EncryptorComponentBuilder : IComponentBuilder
    {
        public EncryptorComponentBuilder(EncryptorComponentOptions configuration = null)
        {
            Configuration = configuration ?? new EncryptorComponentOptions();
        }

        public EncryptorComponentOptions Configuration { get; set; }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var result = new EncryptorComponent(Configuration);

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }
}
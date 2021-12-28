using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public interface IComponentBuilder
    {
        Task<CloudEventsComponent> Build(ComponentFactoryContext context);
    }
}

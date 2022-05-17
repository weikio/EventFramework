using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventAggregatorComponentBuilder : IComponentBuilder
    {
        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var aggr = context.ServiceProvider.GetRequiredService<ICloudEventAggregator>();

            var result = new CloudEventsComponent(async ev =>
            {
                await aggr.Publish(ev);

                return ev;
            });

            return Task.FromResult(result);
        }
    }
}
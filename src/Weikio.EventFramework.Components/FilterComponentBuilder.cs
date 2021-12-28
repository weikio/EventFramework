using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public interface IComponentBuilder
    {
        Task<CloudEventsComponent> Build(ComponentFactoryContext context);
    }

    public class FilterComponentBuilder : IComponentBuilder
    {
        private readonly Func<CloudEvent, Filter> _filter;

        public FilterComponentBuilder(Predicate<CloudEvent> filter)
        {
            Filter Result(CloudEvent ev)
            {
                if (filter(ev))
                {
                    return Filter.Skip;
                }

                return Filter.Continue;
            }

            _filter = Result;
        }

        public FilterComponentBuilder(Func<CloudEvent, Filter> filter)
        {
            _filter = filter;
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var component = new CloudEventsComponent(ev =>
            {
                if (_filter(ev) == Filter.Skip)
                {
                    return null;
                }

                return ev;
            });

            return Task.FromResult(component);
        }
    }
}

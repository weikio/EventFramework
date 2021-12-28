using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
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

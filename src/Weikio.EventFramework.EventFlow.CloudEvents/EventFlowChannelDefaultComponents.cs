using System;
using System.Collections.Generic;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowChannelDefaultComponents
    {
        public Func<EventFlow, EventFlowInstanceOptions, List<CloudEventsComponent>> ComponentsFactory { get; set; } =
            (flow, options) =>
            {
                var result = new List<CloudEventsComponent> { new AddExtensionComponent(ev => new EventFrameworkEventFlowEventExtension(options.Id)) };

                return result;
            };
    }
}

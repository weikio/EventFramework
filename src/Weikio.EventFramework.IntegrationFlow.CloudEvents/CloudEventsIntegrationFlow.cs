using System;
using System.Runtime.CompilerServices;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class CloudEventsIntegrationFlow : IntegrationFlowBase<CloudEvent>
    {
        public Type EventSourceType { get; set; }
        public string Source { get; set; }
    }
}

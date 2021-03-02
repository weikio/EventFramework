using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelOptions
    {
        public string Name { get; set; }
        public List<Func<CloudEvent, Task>> Endpoints { get; set; } = new List<Func<CloudEvent, Task>>();
        public Action<CloudEvent> Endpoint { get; set; }
        public List<Func<CloudEvent, CloudEvent>> Components { get; set; } = new List<Func<CloudEvent, CloudEvent>>();
        public ILoggerFactory LoggerFactory { get; set; }
    }
}
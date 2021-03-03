using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannel : DataflowChannelBase<object, CloudEvent>
    {
        public DataflowChannel(CloudEventsDataflowChannelOptions options) : base(new DataflowChannelOptionsBase<object, CloudEvent>()
        {
            Components = options.Components,
            Endpoint = options.Endpoint,
            Endpoints = options.Endpoints,
            Name = options.Name,
            LoggerFactory = options.LoggerFactory,
            AdapterLayerBuilder = options.AdapterLayerBuilder,
            ComponentLayerBuilder = options.ComponentLayerBuilder
        })
        {
        }

        public DataflowChannel(DataflowChannelOptionsBase<object, CloudEvent> options) : base(options)
        {
        }

        public DataflowChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null) : this(
            new CloudEventsDataflowChannelOptions() { Name = name, Endpoint = endpoint })
        {
        }
    }
}

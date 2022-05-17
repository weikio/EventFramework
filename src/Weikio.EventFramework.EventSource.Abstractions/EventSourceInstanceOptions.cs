using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public class EventSourceInstanceOptions
    {
        public EventSourceDefinition EventSourceDefinition { get; set; }
        public TimeSpan? PollingFrequency { get; set; }
        public string CronExpression { get; set; }
        public MulticastDelegate Configure { get; set; }
        public bool Autostart { get; set; }
        public bool RunOnce { get; set; }
        public object Configuration { get; set; }
        public string TargetChannelName { get; set; }
        public string Id { get; set; }
        public Action<CloudEventsChannelOptions> ConfigureChannel { get; set; } = options => { };
        public bool PersistState { get; set; } = true;

        public Func<IServiceProvider, IEventSourceInstanceStorageFactory> EventSourceInstanceDataStoreFactory { get; set; } = provider =>
        {
            var result = provider.GetRequiredService<IEventSourceInstanceStorageFactory>();

            return result;
        };
        
        public Func<IServiceProvider, EventSourceInstance, Type, IEventSourceInstanceDataStore> EventSourceInstanceDataStore { get; set; }
        
        public Type EventSourceInstanceDataStoreType { get; set; }

        public bool PublishToChannel { get; set; } = true;
    }
}

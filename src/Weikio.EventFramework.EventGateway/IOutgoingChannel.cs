using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventGateway
{
    public interface IOutgoingChannel : IChannel
    {
    }

    // public static class ChannelBuilderExtensions
    // {
    //     public static ChannelBuilder Channel(this ChannelBuilder channelBuilder, string channelName)
    //     {
    //         
    //     }
    //
    //     public static ChannelBuilder Transform(this ChannelBuilder channelBuilder, Func<CloudEvent, CloudEvent> transform)
    //     {
    //         
    //     }
    //     public static ChannelBuilder Filter(this ChannelBuilder channelBuilder, Predicate<CloudEvent> predicate)
    //     {
    //         
    //     }
    // }

    // public class ChannelBuilder
    // {
    //     private bool _isPubSub = true;
    //     private List<IDataflowBlock> _blocks = new List<IDataflowBlock>();
    //
    //     public static ChannelBuilder Build()
    //     {
    //         return new ChannelBuilder();
    //     }
    //
    //     public IChannel Create()
    //     {
    //         var logger = new TransformBlock<CloudEvent, CloudEvent>(ev =>
    //         {
    //             Debug.WriteLine(ev.ToJson());
    //
    //             return ev;
    //         });
    //     }
    //
    //     public void AddComponent(IDataflowBlock block)
    //     {
    //         
    //     }
    // }

    public class EventAggregatorComponent
    {
        private readonly ICloudEventAggregator _eventAggregator;

        public EventAggregatorComponent(ICloudEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public ITargetBlock<CloudEvent> Create()
        {
            var result = new ActionBlock<CloudEvent>(async ev =>
            {
                await _eventAggregator.Publish(ev);
            });

            return result;
        }
    }

    public class LoggerComponent
    {
        private readonly ILogger<LoggerComponent> _logger;

        public LoggerComponent(ILogger<LoggerComponent> logger)
        {
            _logger = logger;
        }

        public Task<CloudEvent> Handle(CloudEvent ev)
        {
            var msg = ev.ToJson();
            _logger.LogDebug(msg);
            
            return Task.FromResult(ev);
        }

        public IDataflowBlock Create()
        {
            var result = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            {
                _logger.LogDebug(ev.ToJson());

                return ev;
            });

            return result;
        }
    }

    public class DefaultChannelFactory : IChannelFactory
    {
        public IChannel Create()
        {
            return new DataflowChannel();
        }
    }

    public interface IChannelFactory
    {
        IChannel Create();
    }

    public interface IChannelBuilder
    {
        IChannel Create(string channelName = ChannelName.Default);
    }

    public class DefaultChannelBuilder : IChannelBuilder
    {
        public IChannel Create(string channelName = ChannelName.Default)
        {
            return new DataflowChannel(channelName);
        }
    }
}

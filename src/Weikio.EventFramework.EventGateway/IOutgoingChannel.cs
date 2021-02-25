using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway
{
    public interface IOutgoingChannel : IChannel
    {
    }

    public static class ChannelBuilderExtensions
    {
        public static ChannelBuilder Channel(this ChannelBuilder channelBuilder, string channelName)
        {
            
        }

        public static ChannelBuilder Transform(this ChannelBuilder channelBuilder, Func<CloudEvent, CloudEvent> transform)
        {
            
        }
        public static ChannelBuilder Filter(this ChannelBuilder channelBuilder, Predicate<CloudEvent> predicate)
        {
            
        }
    }

    public class ChannelBuilder
    {
        private bool _isPubSub = true;
        private List<IDataflowBlock> _blocks = new List<IDataflowBlock>();

        public static ChannelBuilder Build()
        {
            return new ChannelBuilder();
        }

        public IChannel Create()
        {
            var logger = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            {
                Debug.WriteLine(ev.ToJson());

                return ev;
            });
            
            
        }

        public void AddBlock(IDataflowBlock block)
        {
            
        }
    }
    
    public class DataflowChannel : IOutgoingChannel, IDisposable
    {
        private readonly ICloudEventChannelManager _channelManager;
        private readonly string _targetChannelName;
        private List<IDataflowBlock> _blocks = new List<IDataflowBlock>();
        private ITargetBlock<CloudEvent> _startingPoint;
        public string Name { get; } = "test";

        public DataflowChannel(ICloudEventChannelManager channelManager, string name, string targetChannelName)
        {
            Name = name;
            _channelManager = channelManager;
            _targetChannelName = targetChannelName;

            var logger = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            {
                Debug.WriteLine(ev.ToJson());

                return ev;
            });

            _startingPoint = logger;
            _blocks.Add(logger);
            
            var transform = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            {
                ev.Subject = "transformed";

                return ev;
            });

            _blocks.Add(transform);
            
            var broadCast = new BroadcastBlock<CloudEvent>(ev => ev);
            
            _blocks.Add(broadCast);
            
            var outputAction = new ActionBlock<CloudEvent>(async ev =>
            {
                var local = _channelManager.Get(_targetChannelName);
                await local.Send(ev);
            });

            _blocks.Add(outputAction);
            
            var dataflowLinkOptions = new DataflowLinkOptions() { PropagateCompletion = true };
            logger.LinkTo(transform, linkOptions: dataflowLinkOptions);
            transform.LinkTo(broadCast, linkOptions: dataflowLinkOptions);
            broadCast.LinkTo(outputAction, linkOptions: dataflowLinkOptions);
        }
        
        public async Task Send(CloudEvent cloudEvent)
        {
            await _startingPoint.SendAsync(cloudEvent);
        }

        public void Dispose()
        {
            _startingPoint.Complete();
            _startingPoint.Completion.Wait();
        }
    }
}

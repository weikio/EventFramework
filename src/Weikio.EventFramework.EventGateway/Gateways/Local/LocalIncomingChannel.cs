using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventGateway.Gateways.Local
{
    public class LocalIncomingChannel : IIncomingChannel
    {
        public LocalIncomingChannel(string name, ChannelReader<CloudEvent> reader, ChannelWriter<CloudEvent> writer, int? readerCount = 1)
        {
            Name = name;
            Reader = reader;
            Writer = writer;
            ReaderCount = readerCount.GetValueOrDefault();
        }

        public string Name { get; }
        public async Task<bool> Send(object cloudEvent)

        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(IChannel channel)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IChannel channel)
        {
            throw new NotImplementedException();
        }

        public ChannelWriter<CloudEvent> Writer { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }
    //
    // public class NewLocalChannel : IChannel, IDisposable
    // {
    //     private readonly ICloudEventAggregator _cloudEventAggregator;
    //     public string Name { get; } = "local";
    //     private readonly ICloudEventChannelManager _channelManager;
    //     private readonly string _targetChannelName;
    //     private List<IDataflowBlock> _blocks = new List<IDataflowBlock>();
    //     private ITargetBlock<CloudEvent> _startingPoint;
    //     
    //     public NewLocalChannel(ICloudEventAggregator cloudEventAggregator)
    //     {
    //         _cloudEventAggregator = cloudEventAggregator;
    //
    //         var logger = new TransformBlock<CloudEvent, CloudEvent>(ev =>
    //         {
    //             Debug.WriteLine(ev.ToJson());
    //
    //             return ev;
    //         });
    //         
    //         _startingPoint = logger;
    //         _blocks.Add(logger);
    //         
    //         var broadCast = new BroadcastBlock<CloudEvent>(ev => ev);
    //         
    //         _blocks.Add(broadCast);
    //         
    //         var outputAction = new ActionBlock<CloudEvent>(async ev =>
    //         {
    //             await _cloudEventAggregator.Publish(ev);
    //         });
    //
    //         _blocks.Add(outputAction);
    //         var dataflowLinkOptions = new DataflowLinkOptions() { PropagateCompletion = true };
    //         logger.LinkTo(broadCast, linkOptions: dataflowLinkOptions);
    //         broadCast.LinkTo(outputAction, linkOptions: dataflowLinkOptions);
    //     }
    //
    //     public async Task Send(object cloudEvent)
    //     {
    //         await _startingPoint.SendAsync(cloudEvent);
    //     }
    //     
    //     public void Dispose()
    //     {
    //         _startingPoint.Complete();
    //         _startingPoint.Completion.Wait();
    //     }
    // }
}

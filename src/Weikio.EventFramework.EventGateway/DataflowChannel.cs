﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.EventGateway
{
    public class DataflowChannel : IOutgoingChannel, IDisposable, IAsyncDisposable
    {
        private readonly ActionBlock<CloudEvent> _endpoint;
        private readonly ICloudEventChannelManager _channelManager;
        private readonly string _targetChannelName;
        private List<IDataflowBlock> _blocks = new List<IDataflowBlock>();
        private IPropagatorBlock<object, object> _startingPoint;
        private List<IDataflowBlock> _adapterLayerBlocks = new List<IDataflowBlock>();
        private List<IDataflowBlock> _splitterBlocks = new List<IDataflowBlock>();

        public string Name { get; }

        public DataflowChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (endpoint != null)
            {
                _endpoint = new ActionBlock<CloudEvent>(endpoint, new ExecutionDataflowBlockOptions() { });
            }
            else
            {
                _endpoint = new ActionBlock<CloudEvent>(ev => { });
            }

            Name = name;
            var dataflowLinkOptions = new DataflowLinkOptions() { PropagateCompletion = false };

            _startingPoint = new BufferBlock<object>();

            var objectToCloudEventTransformer = new TransformBlock<object, CloudEvent>(o =>
            {
                var result = CloudEventCreator.Create(o);

                return result;
            });

            var objectSplitter = new TransformManyBlock<object, object>(item =>
            {
                var items = (IEnumerable) item;
                return items.Cast<object>();
            });
            
            var eventsSplitter = new TransformManyBlock<object, object>(item =>
            {
                var items = (IEnumerable<CloudEvent>) item;

                return items;
            });

            var cloudEventToCloudEventTransformer = new TransformBlock<object, CloudEvent>(o => (CloudEvent) o);

            _startingPoint.LinkTo(cloudEventToCloudEventTransformer, dataflowLinkOptions, o =>
            {
                return o is CloudEvent;
            });
            
            _startingPoint.LinkTo(eventsSplitter, dataflowLinkOptions, o =>
            {
                return o is IEnumerable<CloudEvent>;
            });

            _startingPoint.LinkTo(objectSplitter, dataflowLinkOptions, o =>
            {
                return o is IEnumerable;
            });

            _startingPoint.LinkTo(objectToCloudEventTransformer, dataflowLinkOptions, o =>
            {
                return o is CloudEvent == false;
            });

            objectSplitter.LinkTo(objectToCloudEventTransformer, dataflowLinkOptions);
            eventsSplitter.LinkTo(cloudEventToCloudEventTransformer, dataflowLinkOptions);
            
            _adapterLayerBlocks.Add(objectToCloudEventTransformer);
            _adapterLayerBlocks.Add(cloudEventToCloudEventTransformer);
            
            _splitterBlocks.Add(objectSplitter);
            _splitterBlocks.Add(eventsSplitter);

            cloudEventToCloudEventTransformer.LinkTo(_endpoint);
            objectToCloudEventTransformer.LinkTo(_endpoint);

            // _startingPoint = new TransformBlock<CloudEvent, CloudEvent>(ev => ev);
        }

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

            // _startingPoint = logger;
            // _blocks.Add(logger);
            //
            // var transform = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            // {
            //     ev.Subject = "transformed";
            //
            //     return ev;
            // });
            //
            // _blocks.Add(transform);
            //
            // var broadCast = new BroadcastBlock<CloudEvent>(ev => ev);
            //
            // _blocks.Add(broadCast);
            //
            // var outputAction = new ActionBlock<CloudEvent>(async ev =>
            // {
            //     var local = _channelManager.Get(_targetChannelName);
            //     await local.Send(ev);
            // });
            //
            // _blocks.Add(outputAction);
            //
            // var dataflowLinkOptions = new DataflowLinkOptions() { PropagateCompletion = true };
            // logger.LinkTo(transform, linkOptions: dataflowLinkOptions);
            // transform.LinkTo(broadCast, linkOptions: dataflowLinkOptions);
            // broadCast.LinkTo(outputAction, linkOptions: dataflowLinkOptions);
        }

        public async Task Send(object cloudEvent)
        {
            await _startingPoint.SendAsync(cloudEvent);
        }

        public void Dispose()
        {
            _startingPoint.Complete();
            
            _startingPoint.Complete();
            _startingPoint.Completion.Wait();

            foreach (var splitterBlock in _splitterBlocks)
            {
                splitterBlock.Complete();
            }
            
            Task.WhenAll(_splitterBlocks.Select(x => x.Completion)).Wait();

            foreach (var adapterBlock in _adapterLayerBlocks)
            {
                adapterBlock.Complete();
            }

            Task.WhenAll(_adapterLayerBlocks.Select(x => x.Completion)).Wait();

            _endpoint.Complete();
            _endpoint.Completion.Wait();
        }

        public async ValueTask DisposeAsync()
        {
            _startingPoint.Complete();
            await _startingPoint.Completion;

            foreach (var splitterBlock in _splitterBlocks)
            {
                splitterBlock.Complete();
            }
            
            await Task.WhenAll(_splitterBlocks.Select(x => x.Completion));

            foreach (var adapterBlock in _adapterLayerBlocks)
            {
                adapterBlock.Complete();
            }

            await Task.WhenAll(_adapterLayerBlocks.Select(x => x.Completion));

            _endpoint.Complete();
            await _endpoint.Completion;
        }
    }
}

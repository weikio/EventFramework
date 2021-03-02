using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
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

            var batchObjectToCloudEventTransformer = new TransformBlock<BatchItem, CloudEvent>(o =>
            {
                var result = CloudEventCreator.Create(o.Object, extensions: new ICloudEventExtension[] { new IntegerSequenceExtension(o.Sequence) });

                return result;
            });

            var batchEventToCloudEventTransformer = new TransformBlock<BatchItem, CloudEvent>(o =>
            {
                var ev = (CloudEvent) o.Object;

                var attributes = ev.GetAttributes();
                var containsSequence = attributes.ContainsKey(SequenceExtension.SequenceAttributeName);

                if (containsSequence)
                {
                    return ev;
                }

                attributes.Add(SequenceExtension.SequenceTypeAttributeName, "Integer");
                attributes.Add(SequenceExtension.SequenceAttributeName, o.Sequence);

                return ev;
            });

            var batchSplitter = new TransformManyBlock<object, BatchItem>(item =>
            {
                var items = (IEnumerable) item;

                // Sequence starts from 0, not from 1
                return items.Cast<object>().Select((x, i) => new BatchItem(x, i+1));
            });

            var cloudEventToCloudEventTransformer = new TransformBlock<object, CloudEvent>(o => (CloudEvent) o);

            _startingPoint.LinkTo(cloudEventToCloudEventTransformer, dataflowLinkOptions, o =>
            {
                return o is CloudEvent;
            });

            _startingPoint.LinkTo(batchSplitter, dataflowLinkOptions, o =>
            {
                return o is IEnumerable;
            });

            batchSplitter.LinkTo(batchObjectToCloudEventTransformer, dataflowLinkOptions, item =>
            {
                return (item.Object is CloudEvent) == false;
            });
            
            batchSplitter.LinkTo(batchEventToCloudEventTransformer, dataflowLinkOptions, item =>
            {
                return item.Object is CloudEvent;
            });

            _startingPoint.LinkTo(objectToCloudEventTransformer, dataflowLinkOptions, o =>
            {
                if (o is IEnumerable)
                {
                    return false;
                }

                if (o is CloudEvent)
                {
                    return false;
                }

                return true;
            });


            _adapterLayerBlocks.Add(objectToCloudEventTransformer);
            _adapterLayerBlocks.Add(cloudEventToCloudEventTransformer);
            _adapterLayerBlocks.Add(batchEventToCloudEventTransformer);
            _adapterLayerBlocks.Add(batchObjectToCloudEventTransformer);

            _splitterBlocks.Add(batchSplitter);

            cloudEventToCloudEventTransformer.LinkTo(_endpoint);
            objectToCloudEventTransformer.LinkTo(_endpoint);
            batchObjectToCloudEventTransformer.LinkTo(_endpoint);
            batchEventToCloudEventTransformer.LinkTo(_endpoint);
        }

        private class BatchItem
        {
            public object Object { get; set; }
            public int Sequence { get; set; }

            public BatchItem(object o, int sequence)
            {
                Object = o;
                Sequence = sequence;
            }
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

            await Task.WhenAny(Task.WhenAll(_adapterLayerBlocks.Select(x => x.Completion)), Task.WhenAll(_endpoint.Completion),
                Task.Delay(TimeSpan.FromSeconds(3)));

            _endpoint.Complete();
            await _endpoint.Completion;
        }
    }

    public class DataflowContext
    {
        public object OriginalObject { get; set; }
    }
}

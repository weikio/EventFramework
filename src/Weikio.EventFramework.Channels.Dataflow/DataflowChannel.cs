using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannel : IOutgoingChannel, IDisposable, IAsyncDisposable
    {
        private readonly DataflowChannelOptions _options;
        private readonly ICloudEventChannelManager _channelManager;
        private readonly string _targetChannelName;
        private List<IDataflowBlock> _blocks = new();
        private IPropagatorBlock<object, object> _startingPoint = new BufferBlock<object>(new DataflowBlockOptions() { BoundedCapacity = 15000000 });
        private List<IDataflowBlock> _adapterBlocks = new List<IDataflowBlock>();
        private List<IDataflowBlock> _splitterBlocks = new List<IDataflowBlock>();
        private List<(ITargetBlock<CloudEvent> Block, Predicate<CloudEvent> Predicate)> _endpointBlocks = new();
        private List<TransformBlock<CloudEvent, CloudEvent>> _componentBlocks = new();
        private TransformBlock<CloudEvent, CloudEvent> _firstComponent = null;
        private TransformBlock<CloudEvent, CloudEvent> _lastComponent = null;
        private ILogger _logger;
        private IPropagatorBlock<CloudEvent, CloudEvent> _endpointChannelBlock = new BroadcastBlock<CloudEvent>(null);
        private Dictionary<string, IDisposable> _subscribers = new();
        private Dictionary<string, IChannel> _subscriberChannels = new();

        public string Name
        {
            get
            {
                return _options.Name;
            }
        }

        public DataflowChannel(DataflowChannelOptions options)
        {
            _options = options;

            if (Name == null)
            {
                throw new ArgumentNullException(nameof(Name));
            }

            if (_options.LoggerFactory != null)
            {
                _logger = _options.LoggerFactory.CreateLogger(typeof(DataflowChannel));
            }
            else
            {
                _logger = new NullLogger<DataflowChannel>();
            }

            if (options.Endpoint != null)
            {
                _endpointBlocks.Add((new ActionBlock<CloudEvent>(options.Endpoint), ev => true));
            }
            else
            {
                _endpointBlocks.Add((new ActionBlock<CloudEvent>(ev => { }), ev => true));
            }

            if (options.Endpoints?.Any() == true)
            {
                foreach (var endpointAction in options.Endpoints)
                {
                    _endpointBlocks.Add((new ActionBlock<CloudEvent>(endpointAction.Func), endpointAction.Predicate));
                }
            }

            var propageteLink = new DataflowLinkOptions() { PropagateCompletion = true };

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
                return items.Cast<object>().Select((x, i) => new BatchItem(x, i + 1));
            });

            var cloudEventToCloudEventTransformer = new TransformBlock<object, CloudEvent>(o => (CloudEvent) o);

            _startingPoint.LinkTo(cloudEventToCloudEventTransformer, propageteLink, o =>
            {
                return o is CloudEvent;
            });

            _startingPoint.LinkTo(batchSplitter, propageteLink, o =>
            {
                return o is IEnumerable;
            });

            batchSplitter.LinkTo(batchObjectToCloudEventTransformer, propageteLink, item =>
            {
                return (item.Object is CloudEvent) == false;
            });

            batchSplitter.LinkTo(batchEventToCloudEventTransformer, propageteLink, item =>
            {
                return item.Object is CloudEvent;
            });

            _startingPoint.LinkTo(objectToCloudEventTransformer, propageteLink, o =>
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

            _adapterBlocks.Add(objectToCloudEventTransformer);
            _adapterBlocks.Add(cloudEventToCloudEventTransformer);
            _adapterBlocks.Add(batchEventToCloudEventTransformer);
            _adapterBlocks.Add(batchObjectToCloudEventTransformer);

            _splitterBlocks.Add(batchSplitter);

            foreach (var optionsComponent in options.Components)
            {
                var transformBlock = new TransformBlock<CloudEvent, CloudEvent>(ev =>
                {
                    try
                    {
                        if (optionsComponent.Predicate(ev))
                        {
                            return optionsComponent.Func.Invoke(ev);
                        }

                        return ev;
                    }
                    catch (Exception e)
                    {
                        // TODO Component logging
                        // ignored
                    }

                    return null;
                });

                _componentBlocks.Add(transformBlock);
            }

            for (var i = 0; i < _componentBlocks.Count; i++)
            {
                var block = _componentBlocks[i];

                if (_firstComponent == null)
                {
                    _firstComponent = block;
                }

                _lastComponent = block;

                block.LinkTo(DataflowBlock.NullTarget<CloudEvent>(), propageteLink, ev => ev == null);

                if (i + 1 < _componentBlocks.Count)
                {
                    var nextBlock = _componentBlocks[i + 1];
                    block.LinkTo(nextBlock, propageteLink, ev => ev != null);
                }
            }

            if (_firstComponent == null)
            {
                _firstComponent = new TransformBlock<CloudEvent, CloudEvent>(ev => ev);
                _lastComponent = new TransformBlock<CloudEvent, CloudEvent>(ev => ev);

                _firstComponent.LinkTo(_lastComponent, propageteLink);
                _componentBlocks.Add(_firstComponent);
                _componentBlocks.Add(_lastComponent);
            }

            cloudEventToCloudEventTransformer.LinkTo(_firstComponent);
            objectToCloudEventTransformer.LinkTo(_firstComponent);
            batchObjectToCloudEventTransformer.LinkTo(_firstComponent);
            batchEventToCloudEventTransformer.LinkTo(_firstComponent);

            // var subscriberPublishBlock = new ActionBlock<CloudEvent>(async ev =>
            // {
            //     foreach (var subscriber in _subscriberChannels)
            //     {
            //         var status = await subscriber.Value.Send(ev);
            //
            //         if (status == false)
            //         {
            //             _logger.LogError("Failed to send message to subscriber");
            //         }
            //     }
            // });
            //
            // _endpointBlocks.Add(subscriberPublishBlock);

            foreach (var endpointBlock in _endpointBlocks)
            {
                _endpointChannelBlock.LinkTo(endpointBlock.Block, propageteLink, endpointBlock.Predicate);
            }

            _lastComponent.LinkTo(_endpointChannelBlock, ev => ev != null);

            _lastComponent.LinkTo(DataflowBlock.NullTarget<CloudEvent>(), ev => ev == null);
        }

        public DataflowChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null) : this(
            new DataflowChannelOptions() { Name = name, Endpoint = endpoint })
        {
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
            // Name = name;
            _channelManager = channelManager;
            _targetChannelName = targetChannelName;

            var logger = new TransformBlock<CloudEvent, CloudEvent>(ev =>
            {
                Console.WriteLine(ev.ToJson());

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

        public async Task<bool> Send(object cloudEvent)
        {
            return await _startingPoint.SendAsync(cloudEvent);
        }

        public void Subscribe(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (channel is DataflowChannel == false)
            {
                throw new Exception("Only dataflow channel is supported for subscribe");
            }

            var dataflowChannel = (DataflowChannel) channel;

            var link = _endpointChannelBlock.LinkTo(dataflowChannel._startingPoint, new DataflowLinkOptions() { });

            _subscribers.Add(channel.Name, link);

            // _subscriberChannels.Add(channel.Name, dataflowChannel);
        }

        public void Unsubscribe(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var link = _subscribers.FirstOrDefault(x => string.Equals(x.Key, channel.Name, StringComparison.InvariantCultureIgnoreCase));

            if (link.Key == null)
            {
                // Todo: Should we throw if unsubscribing unknown channel
                return;
            }

            link.Value.Dispose();

            _subscribers.Remove(link.Key);
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing channel {ChannelName}", Name);
            _startingPoint.Complete();

            var timeoutInSeconds = 180;

            Task.WhenAny(
                Task.WhenAll(_startingPoint.Completion),
                Task.WhenAll(_splitterBlocks.Select(x => x.Completion)),
                Task.WhenAll(_adapterBlocks.Select(x => x.Completion)),
                Task.WhenAll(_componentBlocks.Select(x => x.Completion)),
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds))).Wait();

            foreach (var splitterBlock in _splitterBlocks)
            {
                splitterBlock.Complete();
            }

            Task.WhenAny(
                Task.WhenAll(_splitterBlocks.Select(x => x.Completion)),
                Task.WhenAll(_adapterBlocks.Select(x => x.Completion)),
                Task.WhenAll(_componentBlocks.Select(x => x.Completion)),
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds))).Wait();

            foreach (var adapterBlock in _adapterBlocks)
            {
                adapterBlock.Complete();
            }

            Task.WhenAny(
                Task.WhenAll(_adapterBlocks.Select(x => x.Completion)),
                Task.WhenAll(_componentBlocks.Select(x => x.Completion)),
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds))).Wait();

            foreach (var componentBlock in _componentBlocks)
            {
                componentBlock.Complete();

                Task.WhenAny(
                    Task.WhenAll(componentBlock.Completion),
                    Task.WhenAll(_endpointChannelBlock.Completion),
                    Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds))).Wait();
            }

            _endpointChannelBlock.Complete();

            Task.WhenAny(
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds))).Wait();

            Task.WhenAny(
                Task.WhenAll(_endpointBlocks.Select(x => x.Block.Completion)),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds))).Wait();

            if (_subscribers?.Any() == true)
            {
                foreach (var subscriber in _subscribers)
                {
                    subscriber.Value.Dispose();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Disposing channel {ChannelName} asynchronously", Name);

            _startingPoint.Complete();
            await _startingPoint.Completion;

            var timeoutInSeconds = 180;

            foreach (var splitterBlock in _splitterBlocks)
            {
                splitterBlock.Complete();

                await Task.WhenAny(
                    Task.WhenAll(splitterBlock.Completion),
                    Task.WhenAll(_adapterBlocks.Select(x => x.Completion)),
                    Task.WhenAll(_componentBlocks.Select(x => x.Completion)),
                    Task.WhenAll(_endpointChannelBlock.Completion),
                    Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds)));
            }

            foreach (var adapterBlock in _adapterBlocks)
            {
                adapterBlock.Complete();
            }

            await Task.WhenAny(
                Task.WhenAll(_adapterBlocks.Select(x => x.Completion)),
                Task.WhenAll(_componentBlocks.Select(x => x.Completion)),
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds)));

            foreach (var componentBlock in _componentBlocks)
            {
                componentBlock.Complete();

                await Task.WhenAny(
                    Task.WhenAll(componentBlock.Completion),
                    Task.WhenAll(_endpointChannelBlock.Completion),
                    Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds)));
            }

            _endpointChannelBlock.Complete();

            await Task.WhenAny(
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds)));

            await Task.WhenAny(
                Task.WhenAll(_endpointBlocks.Select(x => x.Block.Completion)),
                Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds)));

            if (_subscribers?.Any() == true)
            {
                foreach (var subscriber in _subscribers)
                {
                    subscriber.Value.Dispose();
                }
            }
        }
    }
}

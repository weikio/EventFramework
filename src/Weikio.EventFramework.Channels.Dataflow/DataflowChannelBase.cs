using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DataflowBlock = System.Threading.Tasks.Dataflow.DataflowBlock;
using DataflowLinkOptions = System.Threading.Tasks.Dataflow.DataflowLinkOptions;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public abstract class DataflowChannelBase<TInput, TOutput> : IOutgoingChannel, IDisposable, IAsyncDisposable
    {
        protected readonly DataflowChannelOptionsBase<TInput, TOutput> _options;
        protected readonly IPropagatorBlock<TInput, TInput> _startingPoint =
            new BufferBlock<TInput>();

        protected List<(ITargetBlock<TOutput> Block, Predicate<TOutput> Predicate)> _endpointBlocks = new();
        protected ILogger _logger;

        protected IPropagatorBlock<TOutput, TOutput> _endpointChannelBlock;

        protected DataflowLayerGeneric<TInput, TOutput> _adapterLayer;
        protected DataflowLayerGeneric<TOutput, TOutput> _componentLayer;

        protected Dictionary<string, IDisposable> _subscribers = new();

        public const int TimeoutInSeconds = 180;

        protected ArrayList Layers { get; set; } = new();

        public string Name
        {
            get
            {
                return _options.Name;
            }
        }

        public DataflowChannelBase(DataflowChannelOptionsBase<TInput, TOutput> options)
        {
            _options = options;

            if (Name == null)
            {
                throw new ArgumentNullException(nameof(Name));
            }

            if (_options.IsPubSub)
            {
                _endpointChannelBlock = new BroadcastBlock<TOutput>(null);
            }
            else
            {
                _endpointChannelBlock = new BufferBlock<TOutput>();
            }

            if (_options.LoggerFactory != null)
            {
                _logger = _options.LoggerFactory.CreateLogger(typeof(DataflowChannelBase<TInput, TOutput>));
            }
            else
            {
                _logger = new NullLogger<DataflowChannelBase<TInput, TOutput>>();
            }

            if (options.Endpoint != null)
            {
                _endpointBlocks.Add((new ActionBlock<TOutput>(options.Endpoint), ev => true));
            }

            if (options.Endpoints?.Any() == true)
            {
                foreach (var endpointAction in options.Endpoints)
                {
                    _endpointBlocks.Add((new ActionBlock<TOutput>(endpointAction.Func), endpointAction.Predicate));
                }
            }
            else
            {
                _endpointChannelBlock.LinkTo(DataflowBlock.NullTarget<TOutput>(), _ => _subscribers?.Any() != true);
            }
            
            var propageteLink = new DataflowLinkOptions() { PropagateCompletion = true };

            _adapterLayer = _options.AdapterLayerBuilder.Invoke();
            _componentLayer = _options.ComponentLayerBuilder.Invoke(_options);
            
            _startingPoint.LinkTo(_adapterLayer.Layer, new DataflowLinkOptions() { PropagateCompletion = false });
            _adapterLayer.Layer.LinkTo(_componentLayer.Layer, new DataflowLinkOptions() { PropagateCompletion = false });
            
            foreach (var endpointBlock in _endpointBlocks)
            {
                _endpointChannelBlock.LinkTo(endpointBlock.Block, propageteLink, endpointBlock.Predicate);
            }
            
            _componentLayer.Layer.LinkTo(_endpointChannelBlock, ev => ev != null);
            _componentLayer.Layer.LinkTo(DataflowBlock.NullTarget<TOutput>(), ev => ev == null);
        }

        public DataflowChannelBase(string name = ChannelName.Default, Action<TOutput> endpoint = null) : this(
            new DataflowChannelOptionsBase<TInput, TOutput>() { Name = name, Endpoint = endpoint })
        {
        }

        public async Task<bool> Send(object cloudEvent)
        {
            return await _startingPoint.SendAsync((TInput)cloudEvent);
        }

        public void Subscribe(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            async Task SendToChannel(TOutput msg)
            {
                await channel.Send(msg);
            }

            var block = new ActionBlock<TOutput>(async output =>
            {
                await SendToChannel(output);
            });

            var link = _endpointChannelBlock.LinkTo(block, new DataflowLinkOptions {PropagateCompletion = true}, obj => obj != null);
            
            _subscribers.Add(channel.Name, link);
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

            Task.WhenAny(
                Task.WhenAll(_startingPoint.Completion),
                Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds))).Wait();

            _adapterLayer.Dispose();
            _componentLayer.Dispose();

            _endpointChannelBlock.Complete();

            Task.WhenAny(
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds))).Wait();

            Task.WhenAny(
                Task.WhenAll(_endpointBlocks.Select(x => x.Block.Completion)),
                Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds))).Wait();

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

            await _adapterLayer.DisposeAsync();
            await _componentLayer.DisposeAsync();

            _endpointChannelBlock.Complete();

            await Task.WhenAny(
                Task.WhenAll(_endpointChannelBlock.Completion),
                Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds)));

            await Task.WhenAny(
                Task.WhenAll(_endpointBlocks.Select(x => x.Block.Completion)),
                Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds)));

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

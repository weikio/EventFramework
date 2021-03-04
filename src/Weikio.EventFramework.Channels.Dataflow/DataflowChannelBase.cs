using System;
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
        protected readonly DataflowChannelOptionsBase<TInput, TOutput> Options;
        protected readonly ILogger Logger;
        protected readonly IPropagatorBlock<TInput, TInput> Input = new BufferBlock<TInput>();
        protected readonly DataflowLayerGeneric<TInput, TOutput> AdapterLayer;
        protected readonly DataflowLayerGeneric<TOutput, TOutput> ComponentLayer;
        protected readonly IPropagatorBlock<TOutput, TOutput> EndpointChannelBlock;
        protected readonly List<(ITargetBlock<TOutput> Block, Predicate<TOutput> Predicate)> EndpointBlocks = new();
        protected readonly Dictionary<string, IDisposable> Subscribers = new();
        
        public string Name
        {
            get
            {
                return Options.Name;
            }
        }

        public DataflowChannelBase(DataflowChannelOptionsBase<TInput, TOutput> options)
        {
            Options = options;

            if (Name == null)
            {
                throw new ArgumentNullException(nameof(Name));
            }

            if (Options.IsPubSub)
            {
                EndpointChannelBlock = new BroadcastBlock<TOutput>(null);
            }
            else
            {
                EndpointChannelBlock = new BufferBlock<TOutput>();
            }

            if (Options.LoggerFactory != null)
            {
                Logger = Options.LoggerFactory.CreateLogger(typeof(DataflowChannelBase<TInput, TOutput>));
            }
            else
            {
                Logger = new NullLogger<DataflowChannelBase<TInput, TOutput>>();
            }

            if (options.Endpoint != null)
            {
                EndpointBlocks.Add((new ActionBlock<TOutput>(options.Endpoint), ev => true));
            }

            if (options.Endpoints?.Any() == true)
            {
                foreach (var endpointAction in options.Endpoints)
                {
                    EndpointBlocks.Add((new ActionBlock<TOutput>(endpointAction.Func), endpointAction.Predicate));
                }
            }
            else
            {
                EndpointChannelBlock.LinkTo(DataflowBlock.NullTarget<TOutput>(), _ => Subscribers?.Any() != true);
            }
            
            var propageteLink = new DataflowLinkOptions() { PropagateCompletion = true };

            AdapterLayer = Options.AdapterLayerBuilder.Invoke(Options);
            ComponentLayer = Options.ComponentLayerBuilder.Invoke(Options);
            
            Input.LinkTo(AdapterLayer.Input, new DataflowLinkOptions() { PropagateCompletion = false });
            AdapterLayer.Output.LinkTo(ComponentLayer.Input, new DataflowLinkOptions() { PropagateCompletion = false });
            
            foreach (var endpointBlock in EndpointBlocks)
            {
                EndpointChannelBlock.LinkTo(endpointBlock.Block, propageteLink, endpointBlock.Predicate);
            }
            
            ComponentLayer.Output.LinkTo(EndpointChannelBlock, predicate:ev => ev != null);
            ComponentLayer.Output.LinkTo(DataflowBlock.NullTarget<TOutput>(), predicate: ev => ev == null);
        }

        public DataflowChannelBase(string name = ChannelName.Default, Action<TOutput> endpoint = null) : this(
            new DataflowChannelOptionsBase<TInput, TOutput>() { Name = name, Endpoint = endpoint })
        {
        }

        public async Task<bool> Send(object cloudEvent)
        {
            return await Input.SendAsync((TInput)cloudEvent);
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

            var link = EndpointChannelBlock.LinkTo(block, new DataflowLinkOptions {PropagateCompletion = true}, obj => obj != null);
            
            Subscribers.Add(channel.Name, link);
        }

        public void Unsubscribe(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var link = Subscribers.FirstOrDefault(x => string.Equals(x.Key, channel.Name, StringComparison.InvariantCultureIgnoreCase));

            if (link.Key == null)
            {
                // Todo: Should we throw if unsubscribing unknown channel
                return;
            }

            link.Value.Dispose();

            Subscribers.Remove(link.Key);
        }

        public void Dispose()
        {
            Logger.LogInformation("Disposing channel {ChannelName}", Name);
            Input.Complete();

            Task.WhenAny(
                Task.WhenAll(Input.Completion),
                Task.Delay(Options.Timeout)).Wait();

            AdapterLayer.Dispose();
            ComponentLayer.Dispose();

            EndpointChannelBlock.Complete();

            Task.WhenAny(
                Task.WhenAll(EndpointChannelBlock.Completion),
                Task.Delay(Options.Timeout)).Wait();

            Task.WhenAny(
                Task.WhenAll(EndpointBlocks.Select(x => x.Block.Completion)),
                Task.Delay(Options.Timeout)).Wait();

            if (Subscribers?.Any() == true)
            {
                foreach (var subscriber in Subscribers)
                {
                    subscriber.Value.Dispose();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            Logger.LogInformation("Disposing channel {ChannelName} asynchronously", Name);

            Input.Complete();
            await Input.Completion;

            await AdapterLayer.DisposeAsync();
            await ComponentLayer.DisposeAsync();

            EndpointChannelBlock.Complete();

            await Task.WhenAny(
                Task.WhenAll(EndpointChannelBlock.Completion),
                Task.Delay(Options.Timeout));

            await Task.WhenAny(
                Task.WhenAll(EndpointBlocks.Select(x => x.Block.Completion)),
                Task.Delay(Options.Timeout));

            if (Subscribers?.Any() == true)
            {
                foreach (var subscriber in Subscribers)
                {
                    subscriber.Value.Dispose();
                }
            }
        }
    }
}

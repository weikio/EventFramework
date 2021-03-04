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
    public abstract class DataflowChannelBase<TInput, TOutput> : IOutgoingChannel, IDisposable, IAsyncDisposable where TInput : class where TOutput : class
    {
        protected readonly DataflowChannelOptionsBase<TInput, TOutput> Options;
        protected readonly ILogger Logger;
        protected readonly IPropagatorBlock<TInput, TInput> Input;
        protected readonly DataflowLayerGeneric<TInput, TOutput> AdapterLayer;
        protected readonly DataflowLayerGeneric<TOutput, TOutput> ComponentLayer;
        protected readonly IPropagatorBlock<TOutput, TOutput> EndpointChannelBlock;
        protected readonly List<(ITargetBlock<TOutput> Block, Predicate<TOutput> Predicate)> EndpointBlocks = new();
        protected readonly Dictionary<string, IDisposable> Subscribers = new();

        public List<(InterceptorTypeEnum InterceptorType, IDataflowChannelInterceptor Interceptor)> Interceptors
        {
            get => Options.Interceptors;
        }

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

            Input = CreateInput();
            EndpointChannelBlock = CreateEndpointChannel();

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

            var builtAdapterLayer = Options.AdapterLayerBuilder.Invoke(Options);
            AdapterLayer = InterceptLayout(builtAdapterLayer, InterceptorTypeEnum.PreAdapters, InterceptorTypeEnum.PostAdapters); 
            
            var builtComponentLayer = Options.ComponentLayerBuilder.Invoke(Options);
            ComponentLayer = InterceptLayout(builtComponentLayer, InterceptorTypeEnum.PreComponents, InterceptorTypeEnum.PostComponent); 

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

        private IPropagatorBlock<TInput, TInput> CreateInput()
        {
            var input = new BufferBlock<TInput>();
            var output = new BufferBlock<TInput>();

            var preInterceptorBlock = new TransformBlock<TInput, TInput>(async obj =>
            {
                foreach (var interceptor in Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PreReceive))
                {
                    obj = (TInput) await interceptor.Interceptor.Intercept(obj);
                }

                return obj;
            });

            input.LinkTo(preInterceptorBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            preInterceptorBlock.LinkTo(output, new DataflowLinkOptions() { PropagateCompletion = true });

            var postInterceptorBlock = new TransformBlock<TInput, TInput>(async obj =>
            {
                foreach (var interceptor in Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PostReceive))
                {
                    obj = (TInput) await interceptor.Interceptor.Intercept(obj);
                }

                return obj;
            });
            
            output.LinkTo(postInterceptorBlock, new DataflowLinkOptions() { PropagateCompletion = true });

            var result = DataflowBlock.Encapsulate(input, postInterceptorBlock);

            return result;
        }

        /// <summary>
        /// Wraps the provided layer with pre and optionally post interceptors
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="preType"></param>
        /// <param name="postType"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        private DataflowLayerGeneric<T1, T2> InterceptLayout<T1, T2>(DataflowLayerGeneric<T1, T2> layer, InterceptorTypeEnum preType, InterceptorTypeEnum? postType = null)
        {
            var preInterceptorBlock = new TransformBlock<T1, T1>(async obj =>
            {
                foreach (var interceptor in Interceptors.Where(x => x.Item1 == preType))
                {
                    obj = (T1) await interceptor.Interceptor.Intercept(obj);
                }
        
                return obj;
            });

            TransformBlock<T2, T2> postInterceptorBlock;

            if (postType != null)
            {
                postInterceptorBlock = new TransformBlock<T2, T2>(async obj =>
                {
                    foreach (var interceptor in Interceptors.Where(x => x.Item1 == postType.GetValueOrDefault()))
                    {
                        obj = (T2) await interceptor.Interceptor.Intercept(obj);
                    }
        
                    return obj;
                });
            }
            else
            {
                postInterceptorBlock = new TransformBlock<T2, T2>(arg => arg);
            }

            preInterceptorBlock.LinkTo(layer.Input, new DataflowLinkOptions() { PropagateCompletion = false });
            layer.Output.LinkTo(postInterceptorBlock, new DataflowLinkOptions() { PropagateCompletion = false });

            var wrapped = DataflowBlock.Encapsulate(preInterceptorBlock, postInterceptorBlock);
            
            async Task Complete(TimeSpan timeout)
            {
                preInterceptorBlock.Complete();
                await Task.WhenAny(
                    Task.WhenAll(preInterceptorBlock.Completion),
                    Task.Delay(timeout));

                await layer.DisposeAsync();
                
                postInterceptorBlock.Complete();
                await Task.WhenAny(
                    Task.WhenAll(postInterceptorBlock.Completion),
                    Task.Delay(timeout));
            }

            var result = new DataflowLayerGeneric<T1, T2>(wrapped, Complete);

            return result;
        }
        
        private IPropagatorBlock<TOutput, TOutput> CreateEndpointChannel()
        {
            var input = new BufferBlock<TOutput>();
            IPropagatorBlock<TOutput, TOutput> output = null;
            
            if (Options.IsPubSub)
            {
                output = new BroadcastBlock<TOutput>(null);
            }
            else
            {
                output = new BufferBlock<TOutput>();
            }
            
            var preInterceptorBlock = new TransformBlock<TOutput, TOutput>(async obj =>
            {
                foreach (var interceptor in Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PreEndpoints))
                {
                    obj = (TOutput) await interceptor.Interceptor.Intercept(obj);
                }

                return obj;
            });

            input.LinkTo(preInterceptorBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            preInterceptorBlock.LinkTo(output, new DataflowLinkOptions() { PropagateCompletion = true });

            var result = DataflowBlock.Encapsulate(input, output);

            return result;
        }

        public async Task<bool> Send(object cloudEvent)
        {
            return await Input.SendAsync((TInput)cloudEvent);
        }

        public void AddInterceptor((InterceptorTypeEnum InterceptorType, IDataflowChannelInterceptor Interceptor) interceptor)
        {
            Interceptors.Add((interceptor.InterceptorType, interceptor.Interceptor));
        }

        public void RemoveInterceptor((InterceptorTypeEnum InterceptorType, IDataflowChannelInterceptor Interceptor) interceptor)
        {
            Interceptors.Remove((interceptor.InterceptorType, interceptor.Interceptor));
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

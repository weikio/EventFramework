using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents.Abstractions
{
    public class SequentialLayerBuilder<TOutput> where TOutput: class
    {
        public DataflowLayerGeneric<TOutput, TOutput> Build(List<ChannelComponent<TOutput>> components,
            List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> interceptors)
        {
            var inputBlock = new BufferBlock<TOutput>();
            var outputBlock = new BufferBlock<TOutput>();
            var propageteLink = new DataflowLinkOptions() { PropagateCompletion = true };

            var componentBlocks = new List<TransformBlock<TOutput, TOutput>>();

            async Task<TOutput> Execute(ChannelComponent<TOutput> component, TOutput ev)
            {
                try
                {
                    if (component.Predicate(ev))
                    {
                        // Run pre interceptors
                        if (interceptors != null)
                        {
                            foreach (var interceptor in interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PreComponent))
                            {
                                ev = (TOutput) await interceptor.Interceptor.Intercept(ev);
                            }
                        }
                        
                        var result = await component.Func.Invoke(ev);

                        // Run post interceptors
                        if (interceptors != null)
                        {
                            foreach (var interceptor in interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PostComponent))
                            {
                                ev = (TOutput) await interceptor.Interceptor.Intercept(ev);
                            }
                        }
                        
                        return result;
                    }

                    return ev;
                }
                catch (Exception e)
                {
                    // TODO Component logging
                    // ignored
                }

                return null;
            }
            
            foreach (var optionsComponent in components)
            {
                var transformBlock = new TransformBlock<TOutput, TOutput>(ev => Execute(optionsComponent, ev));

                componentBlocks.Add(transformBlock);
            }

            TransformBlock<TOutput, TOutput> firstComponent = null;
            TransformBlock<TOutput, TOutput> lastComponent = null;

            for (var i = 0; i < componentBlocks.Count; i++)
            {
                var block = componentBlocks[i];

                if (firstComponent == null)
                {
                    firstComponent = block;
                }

                lastComponent = block;

                block.LinkTo(DataflowBlock.NullTarget<TOutput>(), ev => ev == null);

                if (i + 1 < componentBlocks.Count)
                {
                    var nextBlock = componentBlocks[i + 1];
                    block.LinkTo(nextBlock, propageteLink, ev => ev != null);
                }
            }

            if (firstComponent == null)
            {
                inputBlock.LinkTo(outputBlock, propageteLink);
            }
            else
            {
                inputBlock.LinkTo(firstComponent, propageteLink);
                lastComponent.LinkTo(outputBlock, propageteLink);
            }

            async Task Complete(TimeSpan timeout)
            {
                foreach (var componentBlock in componentBlocks)
                {
                    componentBlock.Complete();

                    await Task.WhenAny(
                        Task.WhenAll(componentBlock.Completion),
                        Task.Delay(timeout));
                }
            }

            var resultblock = DataflowBlock.Encapsulate(inputBlock, outputBlock);

            var layer = new DataflowLayerGeneric<TOutput, TOutput>(resultblock, Complete);

            return layer;
        }
    }
}

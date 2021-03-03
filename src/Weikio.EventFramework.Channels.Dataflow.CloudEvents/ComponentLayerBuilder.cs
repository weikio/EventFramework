using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class ComponentLayerBuilder
    {
        public DataflowLayerGeneric<CloudEvent, CloudEvent> Build(DataflowChannelOptionsBase<object, CloudEvent> options)
        {
            var inputBlock = new BufferBlock<CloudEvent>();
            var outputBlock = new BufferBlock<CloudEvent>();
            var propageteLink = new DataflowLinkOptions() { PropagateCompletion = true };

            List<System.Threading.Tasks.Dataflow.TransformBlock<CloudEvent, CloudEvent>> componentBlocks = new();

            foreach (var optionsComponent in options.Components)
            {
                var transformBlock = new System.Threading.Tasks.Dataflow.TransformBlock<CloudEvent, CloudEvent>(ev =>
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
                componentBlocks.Add(transformBlock);
            }

            System.Threading.Tasks.Dataflow.TransformBlock<CloudEvent, CloudEvent> firstComponent = null;
            System.Threading.Tasks.Dataflow.TransformBlock<CloudEvent, CloudEvent> lastComponent = null;

            for (var i = 0; i < componentBlocks.Count; i++)
            {
                var block = componentBlocks[i];

                if (firstComponent == null)
                {
                    firstComponent = block;
                }

                lastComponent = block;

                block.LinkTo(DataflowBlock.NullTarget<CloudEvent>(), ev => ev == null);

                if (i + 1 < componentBlocks.Count)
                {
                    var nextBlock = componentBlocks[i + 1];
                    DataflowBlock.LinkTo(block, nextBlock, propageteLink, ev => ev != null);
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

            var layer = new DataflowLayerGeneric<CloudEvent, CloudEvent>(resultblock, Complete);

            return layer;
        }
    }
}

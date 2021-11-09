using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Weikio.EventFramework.Channels.Dataflow;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    internal class AdapterLayerBuilder
    {
        public DataflowLayerGeneric<object, CloudEvent> Build(CloudEventsChannelOptions options)
        {
            var inputBlock = new BufferBlock<object>();
            
            // // Todo: Interceptors should be created on DataflowChannelBuilder
            // var preInterceptorBlock = new TransformBlock<object, object>(async obj =>
            // {
            //     foreach (var interceptor in options.Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PreAdapters))
            //     {
            //         obj =  await interceptor.Interceptor.Intercept(obj);
            //     }
            //
            //     return obj;
            // });
            //
            // var postInterceptorBlock = new TransformBlock<CloudEvent, CloudEvent>(async obj =>
            // {
            //     foreach (var interceptor in options.Interceptors.Where(x => x.Item1 == InterceptorTypeEnum.PostAdapters))
            //     {
            //         obj = (CloudEvent) await interceptor.Interceptor.Intercept(obj);
            //     }
            //
            //     return obj;
            // });

            var batchSplitter = new TransformManyBlock<object, BatchItem>(item =>
            {
                var items = (IEnumerable) item;

                // Sequence starts from 0, not from 1
                return items.Cast<object>().Select((x, i) => new BatchItem(x, i + 1));
            });

            var cloudEventToCloudEventTransformer = new TransformBlock<object, CloudEvent>(o => (CloudEvent) o);
            
            var objectToCloudEventTransformer = new TransformBlock<object, CloudEvent>(o =>
            {
                var result = CloudEventCreator.Create(o, options.CloudEventCreationOptions);

                return result;
            });

            var batchObjectToCloudEventTransformer = new TransformBlock<BatchItem, CloudEvent>(o =>
            {
                var result = CloudEventCreator.Create(o.Object, extensions: new ICloudEventExtension[] { new IntegerSequenceExtension(o.Sequence) }, options: options.CloudEventCreationOptions);

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

            batchSplitter.LinkTo(batchObjectToCloudEventTransformer, item =>
            {
                return (item.Object is CloudEvent) == false;
            });

            batchSplitter.LinkTo(batchEventToCloudEventTransformer, item =>
            {
                return item.Object is CloudEvent;
            });

            // preInterceptorBlock.LinkTo(inputBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            
            inputBlock.LinkTo(cloudEventToCloudEventTransformer, o =>
            {
                return o is CloudEvent;
            });

            inputBlock.LinkTo(batchSplitter, o =>
            {
                return o is IEnumerable;
            });

            inputBlock.LinkTo(objectToCloudEventTransformer, o =>
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

            var outputBlock = new BufferBlock<CloudEvent>();
            // outputBlock.LinkTo(postInterceptorBlock, new DataflowLinkOptions() { PropagateCompletion = true });

            cloudEventToCloudEventTransformer.LinkTo(outputBlock);
            objectToCloudEventTransformer.LinkTo(outputBlock);
            batchObjectToCloudEventTransformer.LinkTo(outputBlock);
            batchEventToCloudEventTransformer.LinkTo(outputBlock);

            async Task Complete(TimeSpan timeout)
            {
                inputBlock.Complete();

                await Task.WhenAny(
                    Task.WhenAll(inputBlock.Completion),
                    Task.Delay(timeout));

                batchSplitter.Complete();

                await Task.WhenAny(
                    Task.WhenAll(batchSplitter.Completion),
                    Task.Delay(timeout));

                objectToCloudEventTransformer.Complete();
                cloudEventToCloudEventTransformer.Complete();
                batchEventToCloudEventTransformer.Complete();
                batchObjectToCloudEventTransformer.Complete();

                await Task.WhenAny(
                    Task.WhenAll(objectToCloudEventTransformer.Completion, cloudEventToCloudEventTransformer.Completion,
                        batchEventToCloudEventTransformer.Completion, batchObjectToCloudEventTransformer.Completion),
                    Task.Delay(timeout));

                outputBlock.Complete();

                await Task.WhenAny(
                    Task.WhenAll(outputBlock.Completion),
                    Task.Delay(timeout));
            }

            var block = DataflowBlock.Encapsulate(inputBlock, outputBlock);
            var layer = new DataflowLayerGeneric<object, CloudEvent>(block, Complete);

            return layer;
        }
    }
}

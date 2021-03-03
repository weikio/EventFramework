using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowLayerGeneric<TInput, TOutput>
    {
        private readonly Func<TimeSpan, Task> _completionTask;
        public IPropagatorBlock<TInput, TOutput> Layer { get; private set; }

        public DataflowLayerGeneric(IPropagatorBlock<TInput, TOutput> layer, Func<TimeSpan, Task> completionTask = null)
        {
            Layer = layer;

            if (completionTask == null)
            {
                _completionTask = async _ =>
                {
                    Layer.Complete();

                    await Task.WhenAny(
                        Task.WhenAll(Layer.Completion),
                        // Todo: Timeout
                        Task.Delay(TimeSpan.FromSeconds(180)));
                };
            }
            else
            {
                _completionTask = completionTask;
            }
        }

        public void Dispose()
        {
            _completionTask.Invoke(TimeSpan.FromSeconds(180)).Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await _completionTask.Invoke(TimeSpan.FromSeconds(180));
        }

        public void LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            Layer.LinkTo(target, linkOptions);
        }
    }
    
    public class DataflowLayer : IDisposable, IAsyncDisposable
    {
        private readonly Func<TimeSpan, Task> _completionTask;
        public IPropagatorBlock<object, CloudEvent> Layer { get; private set; }

        public DataflowLayer(IPropagatorBlock<object, CloudEvent> layer, Func<TimeSpan, Task> completionTask = null)
        {
            Layer = layer;

            if (completionTask == null)
            {
                _completionTask = async _ =>
                {
                    Layer.Complete();

                    await Task.WhenAny(
                        Task.WhenAll(Layer.Completion),
                        // Todo: Timeout
                        Task.Delay(TimeSpan.FromSeconds(180)));
                };
            }
            else
            {
                _completionTask = completionTask;
            }
        }

        public void Dispose()
        {
            _completionTask.Invoke(TimeSpan.FromSeconds(180)).Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await _completionTask.Invoke(TimeSpan.FromSeconds(180));
        }
    }
}

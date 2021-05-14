using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weikio.EventFramework.Channels.Dataflow.Abstractions
{
    public class DataflowLayerGeneric<TInput, TOutput>
    {
        private readonly Func<TimeSpan, Task> _completionTask;
        private readonly IPropagatorBlock<TInput, TOutput> _layer;

        public ITargetBlock<TInput> Input
        {
            get
            {
                return _layer;
            }
        }

        public ISourceBlock<TOutput> Output
        {
            get
            {
                return _layer;
            }
        }

        public DataflowLayerGeneric(IPropagatorBlock<TInput, TOutput> layer, Func<TimeSpan, Task> completionTask = null)
        {
            _layer = layer;

            if (completionTask == null)
            {
                _completionTask = async _ =>
                {
                    _layer.Complete();

                    await Task.WhenAny(
                        Task.WhenAll(_layer.Completion),

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
            // Todo: Timeout
            _completionTask.Invoke(TimeSpan.FromSeconds(180)).Wait();
        }

        public async ValueTask DisposeAsync()
        {
            // Todo: Timeout
            await _completionTask.Invoke(TimeSpan.FromSeconds(180));
        }
    }
}

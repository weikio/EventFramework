using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowLayerGeneric<TInput, TOutput> 
    {
        private readonly Func<TimeSpan, Task> _completionTask;
        private IPropagatorBlock<TInput, TOutput> Layer { get; }

        public ITargetBlock<TInput> Input
        {
            get
            {
                return Layer;
            }
        }
        
        public ISourceBlock<TOutput> Output
        {
            get
            {
                return Layer;
            }
        }

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
    }
}

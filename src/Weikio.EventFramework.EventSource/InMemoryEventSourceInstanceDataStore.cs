using System;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class InMemoryEventSourceInstanceDataStore : IEventSourceInstanceDataStore
    {
        private object _state = null;
        private bool _hasRun = false;

        public EventSourceInstance EventSourceInstance { get; set; }
        public Type StateType { get; set; }

        public Task<bool> HasRun()
        {
            return Task.FromResult(_hasRun);
        }

        public Task<dynamic> LoadState()
        {
            return Task.FromResult(_state);
        }

        public Task Save(dynamic updatedState)
        {
            _state = updatedState;
            _hasRun = true;

            return Task.CompletedTask;
        }
    }
}

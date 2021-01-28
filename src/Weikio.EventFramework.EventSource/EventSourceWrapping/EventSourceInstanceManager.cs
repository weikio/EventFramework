using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceInstanceOptions
    {
        public EventSourceDefinition EventSourceDefinition { get; set; }
        public TimeSpan? PollingFrequency { get; set; }
        public string CronExpression { get; set; }
        public MulticastDelegate Configure { get; set; }
        public bool Autostart { get; set; }
        public bool RunOnce { get; set; }
    }

    public class EventSourceInstanceManager : List<EsInstance>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EventSourceProvider _eventSourceProvider;

        public EventSourceInstanceManager(IServiceProvider serviceProvider, EventSourceProvider eventSourceProvider,
            IEnumerable<IOptions<EventSourceInstanceOptions>> initialInstances)
        {
            _serviceProvider = serviceProvider;
            _eventSourceProvider = eventSourceProvider;

            foreach (var initialInstance in initialInstances)
            {
                var initialInstanceOptions = initialInstance.Value;

                Create(initialInstanceOptions.EventSourceDefinition, initialInstanceOptions.PollingFrequency, initialInstanceOptions.CronExpression,
                    initialInstanceOptions.Configure);
            }
        }

        public List<EsInstance> GetAll()
        {
            return this;
        }

        public Guid Create(EventSourceInstanceOptions options)
        {
            return Create(options.EventSourceDefinition, options.PollingFrequency, options.CronExpression, options.Configure);
        }
        
        public Guid Create(EventSourceDefinition eventSourceDefinition, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            var eventSource = _eventSourceProvider.Get(eventSourceDefinition);

            var instance = new EsInstance(eventSource, pollingFrequency, cronExpression, configure, null, null);
            var result = instance.Id;

            return result;
        }

        public async Task Start(Guid eventSourceInstanceId)
        {
            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }

            inst.Status.UpdateStatus(EventSourceStatusEnum.Starting);

            await inst.Start(_serviceProvider);
        }

        public async Task Stop(Guid eventSourceId)
        {
            var inst = this.FirstOrDefault(x => x.Id == eventSourceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceId);
            }

            inst.Status.UpdateStatus(EventSourceStatusEnum.Stopping);

            await inst.Stop(_serviceProvider);
        }

        public async Task Remove(Guid eventSourceId)
        {
            var inst = this.FirstOrDefault(x => x.Id == eventSourceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceId);
            }

            inst.Status.UpdateStatus(EventSourceStatusEnum.Stopping);

            await inst.Stop(_serviceProvider);

            Remove(inst);
        }
    }
}

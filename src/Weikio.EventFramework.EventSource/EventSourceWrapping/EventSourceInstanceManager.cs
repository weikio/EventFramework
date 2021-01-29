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

    public interface IEventSourceInstanceManager
    {
        List<EsInstance> GetAll();
        Guid Create(EventSourceInstanceOptions options);

        Guid Create(string name, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        Guid Create(string name, Version version, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        Guid Create(EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        Guid Create(EventSourceDefinition eventSourceDefinition, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        Task Start(Guid eventSourceInstanceId);
        Task StartAll();
        Task Stop(Guid eventSourceId);
        Task StopAll();
        Task Remove(Guid eventSourceId);
        Task RemoveAll();
    }

    public class DefaultEventSourceInstanceManager : List<EsInstance>, IEventSourceInstanceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EventSourceProvider _eventSourceProvider;
        private readonly IEventSourceInstanceFactory _instanceFactory;

        public DefaultEventSourceInstanceManager(IServiceProvider serviceProvider, EventSourceProvider eventSourceProvider,
            IEnumerable<IOptions<EventSourceInstanceOptions>> initialInstances, IEventSourceInstanceFactory instanceFactory)
        {
            _serviceProvider = serviceProvider;
            _eventSourceProvider = eventSourceProvider;
            _instanceFactory = instanceFactory;

            // foreach (var initialInstance in initialInstances)
            // {
            //     var initialInstanceOptions = initialInstance.Value;
            //
            //     Create(initialInstanceOptions.EventSourceDefinition, initialInstanceOptions.PollingFrequency, initialInstanceOptions.CronExpression,
            //         initialInstanceOptions.Configure);
            // }
        }

        public List<EsInstance> GetAll()
        {
            return this;
        }

        public Guid Create(EventSourceInstanceOptions options)
        {
            return Create(options.EventSourceDefinition, options.PollingFrequency, options.CronExpression, options.Configure);
        }
        
        public Guid Create(string name, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(name, Version.Parse("1.0.0.0"), pollingFrequency, cronExpression, configure);
        }
        
        public Guid Create(string name, Version version, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            var eventSource = _eventSourceProvider.Get(new EventSourceDefinition(name, version));
            return Create(eventSource, pollingFrequency, cronExpression, configure);
        }

        
        public Guid Create(EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(eventSource.EventSourceDefinition, pollingFrequency, cronExpression, configure);
        }
        
        public Guid Create(EventSourceDefinition eventSourceDefinition, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            var eventSource = _eventSourceProvider.Get(eventSourceDefinition);

            var instance = _instanceFactory.Create(eventSource, pollingFrequency, cronExpression, configure);
            
            Add(instance);
            
            var result = instance.Id;
            return result;
        }

        public async Task StartAll()
        {
            foreach (var eventSourceInstance in this)
            {
                if (eventSourceInstance.Status.Status != EventSourceStatusEnum.Started && eventSourceInstance.Status.Status != EventSourceStatusEnum.Starting)
                {
                    await Start(eventSourceInstance.Id);
                }
            }
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

        public async Task StopAll()
        {
            foreach (var eventSourceInstance in this)
            {
                await Stop(eventSourceInstance.Id);
            }
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

        public async Task RemoveAll()
        {
            foreach (var eventSourceInstance in this)
            {
                await Remove(eventSourceInstance.Id);
            }
        }
    }
}

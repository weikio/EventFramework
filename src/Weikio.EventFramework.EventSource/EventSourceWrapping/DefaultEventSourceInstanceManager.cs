using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
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
            return Create(options.EventSourceDefinition, options.PollingFrequency, options.CronExpression, options.Configure, options.ConfigurePublisherOptions);
        }
        
        public Guid Create(string name, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null )
        {
            return Create(name, Version.Parse("1.0.0.0"), pollingFrequency, cronExpression, configure, configurePublisherOptions);
        }
        
        public Guid Create(string name, Version version, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null)
        {
            var eventSource = _eventSourceProvider.Get(new EventSourceDefinition(name, version));
            
            return Create(eventSource, pollingFrequency, cronExpression, configure, configurePublisherOptions);
        }

        public Guid Create(EventSourceDefinition eventSourceDefinition, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null)
        {
            var eventSource = _eventSourceProvider.Get(eventSourceDefinition);
            return Create(eventSource, pollingFrequency, cronExpression, configure, configurePublisherOptions);
        }
        
        public Guid Create(EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null)
        {
            var instance = _instanceFactory.Create(eventSource, pollingFrequency, cronExpression, configure, configurePublisherOptions);
            
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

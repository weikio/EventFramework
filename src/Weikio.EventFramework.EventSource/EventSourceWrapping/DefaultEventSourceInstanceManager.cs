using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class DefaultEventSourceInstanceManager : List<EsInstance>, IEventSourceInstanceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EventSourceProvider _eventSourceProvider;
        private readonly IEventSourceInstanceFactory _instanceFactory;
        private readonly ILogger<DefaultEventSourceInstanceManager> _logger;

        public DefaultEventSourceInstanceManager(IServiceProvider serviceProvider, EventSourceProvider eventSourceProvider,
            IEventSourceInstanceFactory instanceFactory, ILogger<DefaultEventSourceInstanceManager> logger)
        {
            _serviceProvider = serviceProvider;
            _eventSourceProvider = eventSourceProvider;
            _instanceFactory = instanceFactory;
            _logger = logger;

        }

        public List<EsInstance> GetAll()
        {
            return this;
        }

        public EsInstance Get(Guid id)
        {
            return this.FirstOrDefault(x => x.Id == id);
        }

        public async Task<Guid> Create(EventSourceInstanceOptions options)
        {
            _logger.LogInformation("Creating new event source instance from options {Options}", options);
            
            var eventSource = _eventSourceProvider.Get(options.EventSourceDefinition);
            var instance = _instanceFactory.Create(eventSource, options);

            Add(instance);

            var result = instance.Id;

            if (instance.Options.Autostart)
            {
                await Start(result);
            }

            return result;
        }

        public async Task StartAll()
        {
            _logger.LogDebug("Starting all event source instances");
            
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
            _logger.LogInformation("Stopping event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

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
            _logger.LogDebug("Stopping all event source instances");
            
            foreach (var eventSourceInstance in this)
            {
                await Stop(eventSourceInstance.Id);
            }
        }

        public async Task Stop(Guid eventSourceInstanceId)
        {
            _logger.LogInformation("Stopping event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }

            if (inst.Status != EventSourceStatusEnum.Starting && inst.Status != EventSourceStatusEnum.Started)
            {
                _logger.LogDebug("Event source with id {EventSourceInstanceId} is in status {Status}, no need to stop", eventSourceInstanceId, inst.Status.Status);

                return;
            }

            inst.Status.UpdateStatus(EventSourceStatusEnum.Stopping);

            await inst.Stop(_serviceProvider);
        }

        public async Task Remove(Guid eventSourceInstanceId)
        {
            _logger.LogInformation("Removing event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }

            if (inst.Status == EventSourceStatusEnum.Starting && inst.Status == EventSourceStatusEnum.Started)
            {
                _logger.LogDebug("Event source with id {EventSourceInstanceId} is in status {Status}, stop before removing", eventSourceInstanceId, inst.Status.Status);

                await inst.Stop(_serviceProvider);
            }
            
            inst.Status.UpdateStatus(EventSourceStatusEnum.Removed);
        }

        public async Task RemoveAll()
        {
            _logger.LogDebug("Removing all event source instances");

            foreach (var eventSourceInstance in this)
            {
                await Remove(eventSourceInstance.Id);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceInstanceManager : HashSet<EventSourceInstance>, IEventSourceInstanceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly IEventSourceInstanceFactory _instanceFactory;
        private readonly ILogger<DefaultEventSourceInstanceManager> _logger;
        private readonly IChannelManager _channelManager;
        private readonly IOptionsMonitor<EventSourceOptions> _eventSourceOptionsAccessor;

        public DefaultEventSourceInstanceManager(IServiceProvider serviceProvider, IEventSourceProvider eventSourceProvider,
            IEventSourceInstanceFactory instanceFactory, ILogger<DefaultEventSourceInstanceManager> logger, IChannelManager channelManager, IOptionsMonitor<EventSourceOptions> eventSourceOptionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _eventSourceProvider = eventSourceProvider;
            _instanceFactory = instanceFactory;
            _logger = logger;
            _channelManager = channelManager;
            _eventSourceOptionsAccessor = eventSourceOptionsAccessor;
        }

        public IEnumerable<EventSourceInstance> GetAll()
        {
            return this;
        }

        public EventSourceInstance Get(string id)
        {
            return this.FirstOrDefault(x => x.Id == id);
        }

        public async Task<string> Create(EventSourceInstanceOptions options)
        {
            _logger.LogInformation("Creating new event source instance from options {Options}", options);

            var eventSource = _eventSourceProvider.Get(options.EventSourceDefinition);
            var instance = _instanceFactory.Create(eventSource, options);

            var added = Add(instance);

            if (added == false)
            {
                throw new DuplicateEventSourceInstanceException($"Event source instance with {instance.Id} already exists");
            }

            var result = instance.Id;

            _logger.LogInformation("Created new event source instance with id {Id} from options {Options}", result, options);

            if (instance.Options.Autostart)
            {
                _logger.LogInformation("Event source instance with id {Id} has auto start enabled, starting", result);

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

        public async Task Start(string eventSourceInstanceId)
        {
            _logger.LogDebug("Starting event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }

            inst.Status.UpdateStatus(EventSourceStatusEnum.Starting);

            await inst.Start(_serviceProvider);

            _logger.LogInformation("Started event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);
        }

        public async Task StopAll()
        {
            _logger.LogDebug("Stopping all event source instances");

            foreach (var eventSourceInstance in this)
            {
                await Stop(eventSourceInstance.Id);
            }
        }

        public async Task Stop(string eventSourceInstanceId)
        {
            _logger.LogDebug("Stopping event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }

            if (inst.Status != EventSourceStatusEnum.Starting && inst.Status != EventSourceStatusEnum.Started)
            {
                _logger.LogDebug("Event source with id {EventSourceInstanceId} is in status {Status}, no need to stop", eventSourceInstanceId,
                    inst.Status.Status);

                return;
            }

            inst.Status.UpdateStatus(EventSourceStatusEnum.Stopping);

            await inst.Stop(_serviceProvider);

            _logger.LogInformation("Stopped event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);
        }

        public async Task Remove(string eventSourceInstanceId)
        {
            _logger.LogInformation("Removing event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }

            if (inst.Status == EventSourceStatusEnum.Starting || inst.Status == EventSourceStatusEnum.Started)
            {
                _logger.LogDebug("Event source with id {EventSourceInstanceId} is in status {Status}, stop before removing", eventSourceInstanceId,
                    inst.Status.Status);

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
        
        public async Task Delete(string eventSourceInstanceId)
        {
            _logger.LogInformation("Deleting event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);

            var inst = this.FirstOrDefault(x => x.Id == eventSourceInstanceId);

            if (inst == null)
            {
                throw new ArgumentException("Unknown event source instance " + eventSourceInstanceId);
            }
            
            if (inst.Status != EventSourceStatusEnum.Removed)
            {
                _logger.LogDebug("Event source with id {EventSourceInstanceId} is in status {Status}, remove before deleting", eventSourceInstanceId,
                    inst.Status.Status);

                await Remove(eventSourceInstanceId);
            }

            Remove(inst);
            
            // We also need to do some cleanup work by deleting the instance's channel
            var eventSourceOptions = _eventSourceOptionsAccessor.CurrentValue;
            var channelName = eventSourceOptions.EventSourceInstanceChannelNameFactory(eventSourceInstanceId);

            var channel = _channelManager.Get(channelName);

            if (channel != null)
            {
                _logger.LogDebug("Deleting internal channel {ChannelName} for event source instance with id {EventSourceInstanceId}", channelName, eventSourceInstanceId);
                _channelManager.Remove(channel);
            }
            
            _logger.LogDebug("Deleted event source instance with id {EventSourceInstanceId}", eventSourceInstanceId);
        }
    }
}

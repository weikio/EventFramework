using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitorCache<JobOptions> _optionsCache;
        private readonly PollingScheduleService _scheduleService;

        public EventSourceInitializer(IServiceProvider serviceProvider, IOptionsMonitorCache<JobOptions> optionsCache, PollingScheduleService scheduleService)
        {
            _serviceProvider = serviceProvider;
            _optionsCache = optionsCache;
            _scheduleService = scheduleService;
        }

        public void Initialize(EventSource eventSource)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            var eventSourceType = eventSource.EventSourceType;
            var eventSourceInstance = eventSource.EventSourceInstance;
            var pollingFrequency = eventSource.PollingFrequency;
            var cronExpression = eventSource.CronExpression;
            var configure = eventSource.Configure;
            var id = eventSource.Id;
            var action = eventSource.Action;

            if (eventSourceType == null && eventSourceInstance != null)
            {
                eventSourceType = eventSourceInstance.GetType();
            }

            var isHostedService = eventSourceType != null && typeof(IHostedService).IsAssignableFrom(eventSourceType);

            var requiresPollingJob = isHostedService == false;

            if (requiresPollingJob)
            {
                if (pollingFrequency == null)
                {
                    var optionsManager = _serviceProvider.GetService<IOptionsMonitor<PollingOptions>>();
                    var options = optionsManager.CurrentValue;

                    pollingFrequency = options.PollingFrequency;
                }

                var schedule = new PollingSchedule(id, pollingFrequency, cronExpression);
                _scheduleService.Add(schedule);
            }

            if (isHostedService)
            {
                var inst = (IHostedService) ActivatorUtilities.CreateInstance(_serviceProvider, eventSourceType);

                if (configure != null)
                {
                    configure.DynamicInvoke(inst);
                }

                inst.StartAsync(new CancellationToken());
            }
            else if (eventSourceType != null)
            {
                var logger = _serviceProvider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
                var factory = new TypeToEventSourceFactory(eventSourceType, id, logger, eventSourceInstance);
           
                // Event source can contain multiple event sources...

                var sources = factory.Create(_serviceProvider);
                foreach (var eventSourceActionWrapper in sources)
                {
                    var childId = eventSourceActionWrapper.Id;

                    var childEventSource = eventSourceActionWrapper.EventSource;
                    var opts = new JobOptions { Action = eventSource.Action, ContainsState = childEventSource.ContainsState };
                    _optionsCache.TryAdd(childId, opts);

                    var schedule = new PollingSchedule(id, pollingFrequency, cronExpression);
                    _scheduleService.Add(schedule);
                }
            }
            else
            {
                var wrapper = _serviceProvider.GetRequiredService<EventSourceActionWrapper>();
                var wrapped = wrapper.Wrap(action);

                var jobOptions = new JobOptions { Action = wrapped.Action, ContainsState = wrapped.ContainsState };

                _optionsCache.TryAdd(id.ToString(), jobOptions);
            }

            eventSource.IsInitialized = true;

            return;
        }
    }
}

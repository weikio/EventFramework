using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.LongPolling;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitorCache<JobOptions> _optionsCache;
        private readonly PollingScheduleService _scheduleService;
        private readonly EventSourceChangeNotifier _changeNotifier;

        public EventSourceInitializer(IServiceProvider serviceProvider, IOptionsMonitorCache<JobOptions> optionsCache, PollingScheduleService scheduleService, EventSourceChangeNotifier changeNotifier)
        {
            _serviceProvider = serviceProvider;
            _optionsCache = optionsCache;
            _scheduleService = scheduleService;
            _changeNotifier = changeNotifier;
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
                
                foreach (var eventSourceActionWrapper in sources.PollingEventSources)
                {
                    var childId = eventSourceActionWrapper.Id;

                    var childEventSource = eventSourceActionWrapper.EventSource;
                    var opts = new JobOptions { Action = childEventSource.Action, ContainsState = childEventSource.ContainsState };
                    _optionsCache.TryAdd(childId, opts);

                    var schedule = new PollingSchedule(childId, pollingFrequency, cronExpression);
                    _scheduleService.Add(schedule);
                }
                
                foreach (var eventSourceActionWrapper in sources.LongPollingEventSources)
                {
                    var method = eventSourceActionWrapper.Source;
                    var poller = method.Invoke();

                    var host = _serviceProvider.GetRequiredService<ILongPollingEventSourceHost>();
                    host.Initialize(poller);

                    host.StartPolling(new CancellationToken());
                }
            }
            else
            {
                var wrapper = _serviceProvider.GetRequiredService<EventSourceActionWrapper>();
                var wrapped = wrapper.Wrap(action);

                var jobOptions = new JobOptions { Action = wrapped.Action, ContainsState = wrapped.ContainsState };

                _optionsCache.TryAdd(id.ToString(), jobOptions);
                
                var schedule = new PollingSchedule(id, pollingFrequency, cronExpression);
                _scheduleService.Add(schedule);
            }

            eventSource.IsInitialized = true;

            _changeNotifier.Notify();
        }
    }
}

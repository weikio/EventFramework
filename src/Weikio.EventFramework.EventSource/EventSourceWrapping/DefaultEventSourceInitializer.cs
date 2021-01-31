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
    public class DefaultEventSourceInitializer : IEventSourceInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitorCache<JobOptions> _optionsCache;
        private readonly PollingScheduleService _scheduleService;
        private readonly ILogger<DefaultEventSourceInitializer> _logger;

        public DefaultEventSourceInitializer(IServiceProvider serviceProvider, IOptionsMonitorCache<JobOptions> optionsCache,
            PollingScheduleService scheduleService, ILogger<DefaultEventSourceInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _optionsCache = optionsCache;
            _scheduleService = scheduleService;
            _logger = logger;
        }

        public EventSourceStatusEnum Initialize(EventSourceInstance eventSourceInstance)
        {
            if (eventSourceInstance == null)
            {
                throw new ArgumentNullException(nameof(eventSourceInstance));
            }

            try
            {
                _logger.LogInformation("Initializing event source with id {Id}", eventSourceInstance.Id);

                eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Initializing, "Initializing");

                var eventSourceType = eventSourceInstance.EventSourceType;
                var instance = eventSourceInstance.Instance;
                var pollingFrequency = eventSourceInstance.PollingFrequency;
                var cronExpression = eventSourceInstance.CronExpression;
                var configure = eventSourceInstance.Configure;
                var id = eventSourceInstance.Id;
                var action = eventSourceInstance.Action;

                if (eventSourceType == null && instance != null)
                {
                    eventSourceType = instance.GetType();
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

                    eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");

                    var cancellationToken = new CancellationTokenSource();
                    inst.StartAsync(cancellationToken.Token);

                    eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Started, "Running");

                    eventSourceInstance.SetCancellationTokenSource(cancellationToken);
                }
                else if (eventSourceType != null)
                {
                    var logger = _serviceProvider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
                    var factory = new TypeToEventSourceFactory(eventSourceType, id, logger, instance, configure);

                    // Event source can contain multiple event sources...

                    var sources = factory.Create(_serviceProvider);
                    eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");

                    foreach (var eventSourceActionWrapper in sources.PollingEventSources)
                    {
                        var childId = eventSourceActionWrapper.Id;

                        var childEventSource = eventSourceActionWrapper.EventSource;
                        var opts = new JobOptions { Action = childEventSource.Action, ContainsState = childEventSource.ContainsState, EventSource = null};
                        _optionsCache.TryAdd(childId, opts);

                        var schedule = new PollingSchedule(childId, pollingFrequency, cronExpression, null);
                        _scheduleService.Add(schedule);
                    }

                    foreach (var eventSourceActionWrapper in sources.LongPollingEventSources)
                    {
                        var method = eventSourceActionWrapper.Source;
                        var poller = method.Invoke();

                        var host = _serviceProvider.GetRequiredService<ILongPollingEventSourceHost>();
                        host.Initialize(eventSourceInstance, poller);
                        
                        var cancellationToken = new CancellationTokenSource();
                        eventSourceInstance.SetCancellationTokenSource(cancellationToken);

                        host.StartPolling(cancellationToken.Token);
                    }
                }
                else
                {
                    var wrapper = _serviceProvider.GetRequiredService<EventSourceActionWrapper>();
                    var wrapped = wrapper.Wrap(action);

                    var jobOptions = new JobOptions { Action = wrapped.Action, ContainsState = wrapped.ContainsState};

                    _optionsCache.TryAdd(id.ToString(), jobOptions);

                    var schedule = new PollingSchedule(id, pollingFrequency, cronExpression, null);
                    _scheduleService.Add(schedule);
                    
                    eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");
                }


            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to initialize event source with id {Id}", eventSourceInstance.Id);
                eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.InitializingFailed, "Failed: " + e);
            }

            return eventSourceInstance.Status.Status;
        }
    }
}

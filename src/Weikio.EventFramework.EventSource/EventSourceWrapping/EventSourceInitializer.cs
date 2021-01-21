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

        public EventSourceStatusEnum Initialize(EventSource eventSource)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            try
            {
                _logger.LogInformation("Initializing event source with id {Id}", eventSource.Id);

                eventSource.Status.UpdateStatus(EventSourceStatusEnum.Initializing, "Initializing");

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

                    eventSource.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");

                    var cancellationToken = new CancellationTokenSource();
                    inst.StartAsync(cancellationToken.Token);

                    eventSource.Status.UpdateStatus(EventSourceStatusEnum.Running, "Running");

                    eventSource.SetCancellationTokenSource(cancellationToken);
                }
                else if (eventSourceType != null)
                {
                    var logger = _serviceProvider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
                    var factory = new TypeToEventSourceFactory(eventSourceType, id, logger, eventSourceInstance);

                    // Event source can contain multiple event sources...

                    var sources = factory.Create(_serviceProvider);
                    eventSource.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");

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
                        host.Initialize(eventSource, poller);
                        
                        var cancellationToken = new CancellationTokenSource();
                        eventSource.SetCancellationTokenSource(cancellationToken);

                        host.StartPolling(cancellationToken.Token);
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
                    
                    eventSource.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");
                }


            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to initialize event source with id {Id}", eventSource.Id);
                eventSource.Status.UpdateStatus(EventSourceStatusEnum.InitializingFailed, "Failed: " + e);
            }

            return eventSource.Status.Status;
        }
    }
}

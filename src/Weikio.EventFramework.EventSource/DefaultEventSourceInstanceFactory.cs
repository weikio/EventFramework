using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.LongPolling;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceInstanceFactory : IEventSourceInstanceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitorCache<JobOptions> _optionsCache;
        private readonly PollingScheduleService _scheduleService;
        private readonly ILogger<DefaultEventSourceInstanceFactory> _logger;
        private readonly EventSourceChangeNotifier _changeNotifier;
        private readonly IOptionsMonitorCache<CloudEventPublisherFactoryOptions> _optionsMonitorCache;
        private readonly IOptionsMonitor<CloudEventPublisherFactoryOptions> _optionsMonitor;
        private readonly ICloudEventPublisherFactory _publisherFactory;

        public DefaultEventSourceInstanceFactory(IServiceProvider serviceProvider, IOptionsMonitorCache<JobOptions> optionsCache,
            PollingScheduleService scheduleService, ILogger<DefaultEventSourceInstanceFactory> logger, EventSourceChangeNotifier changeNotifier, 
            IOptionsMonitorCache<CloudEventPublisherFactoryOptions> optionsMonitorCache, 
            IOptionsMonitor<CloudEventPublisherFactoryOptions> optionsMonitor, ICloudEventPublisherFactory publisherFactory)
        {
            _serviceProvider = serviceProvider;
            _optionsCache = optionsCache;
            _scheduleService = scheduleService;
            _logger = logger;
            _changeNotifier = changeNotifier;
            _optionsMonitorCache = optionsMonitorCache;
            _optionsMonitor = optionsMonitor;
            _publisherFactory = publisherFactory;
        }

        public EsInstance Create(Abstractions.EventSource eventSource, EventSourceInstanceOptions instanceOptions)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            var eventSourceType = eventSource.EventSourceType;
            var instance = eventSource.Instance;
            var id = Guid.NewGuid();
            var action = eventSource.Action;
            Func<IServiceProvider, EsInstance, Task<bool>> start = null;
            Func<IServiceProvider, EsInstance, Task<bool>> stop = null;

            var pollingFrequency = instanceOptions.PollingFrequency;
            var cronExpression = instanceOptions.CronExpression;
            var configurePublisherOptions = instanceOptions.ConfigurePublisherOptions;
            var configure = instanceOptions.Configure;
            
            try
            {
                _logger.LogInformation("Creating event source instance from event source {EventSource}", eventSource);

                if (eventSourceType == null && instance != null)
                {
                    eventSourceType = instance.GetType();
                }

                var isHostedService = eventSourceType != null && typeof(IHostedService).IsAssignableFrom(eventSourceType);

                var requiresPollingJob = isHostedService == false;

                if (requiresPollingJob)
                {
                    if (pollingFrequency == null && cronExpression == null)
                    {
                        var optionsManager = _serviceProvider.GetService<IOptionsMonitor<PollingOptions>>();
                        var options = optionsManager.CurrentValue;

                        pollingFrequency = options.PollingFrequency;
                        cronExpression = options.Cron;
                    }
                }

                if (isHostedService)
                {
                    var cancellationToken = new CancellationTokenSource();

                    IHostedService inst = null;
                    start = (provider, esInstance) =>
                    {
                        var extraParams = new List<object>();
                        if (instanceOptions.Configuration != null)
                        {
                            extraParams.Add(instanceOptions.Configuration);
                        }
                        
                        var publisher = _publisherFactory.CreatePublisher(id.ToString());
                        extraParams.Add(publisher);
                        
                        inst = (IHostedService) ActivatorUtilities.CreateInstance(_serviceProvider, eventSourceType, extraParams.ToArray());

                        if (configure != null)
                        {
                            configure.DynamicInvoke(inst);
                        }
                        
                        _logger.LogDebug("Starting hosted service based event source {EventSourceType} with id {Id}", eventSourceType, id);

                        inst.StartAsync(cancellationToken.Token);
                        esInstance.Status.UpdateStatus(EventSourceStatusEnum.Started);

                        return Task.FromResult(true);
                    };

                    stop = (provider, esInstance) =>
                    {
                        inst.StopAsync(cancellationToken.Token);
                        esInstance.Status.UpdateStatus(EventSourceStatusEnum.Stopped);

                        return Task.FromResult(true);
                    };
                }
                else if (eventSourceType != null)
                {
                    var cancellationToken = new CancellationTokenSource();

                    start = (provider, esInstance) =>
                    {
                        var logger = _serviceProvider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
                        var factory = new TypeToEventSourceFactory(esInstance, logger);

                        // Event source can contain multiple event sources...

                        var sources = factory.Create(_serviceProvider);


                        foreach (var eventSourceActionWrapper in sources.PollingEventSources)
                        {
                            var childId = eventSourceActionWrapper.Id;

                            var childEventSource = eventSourceActionWrapper.EventSource;
                            var opts = new JobOptions { Action = childEventSource.Action, ContainsState = childEventSource.ContainsState, EventSource = esInstance};
                            _optionsCache.TryAdd(childId, opts);

                            var schedule = new PollingSchedule(childId, pollingFrequency, cronExpression, esInstance);
                            _scheduleService.Add(schedule);
                            
                            esInstance.Status.UpdateStatus(EventSourceStatusEnum.Started, "Started polling");
                        }

                        foreach (var eventSourceActionWrapper in sources.LongPollingEventSources)
                        {
                            var method = eventSourceActionWrapper.Source;
                            var poller = method.Invoke();

                            var host = _serviceProvider.GetRequiredService<ILongPollingEventSourceHost>();
                            host.Initialize(esInstance, poller, cancellationToken);

                            // eventSourceInstance.SetCancellationTokenSource(cancellationToken);

                            host.StartPolling(cancellationToken.Token);
                        }

                        _changeNotifier.Notify();

                        return Task.FromResult(true);
                    };

                    stop = (provider, esInstance) =>
                    {
                        var currentPollingSchedule = _scheduleService.FirstOrDefault(x => x.EventSourceInstance.Id == esInstance.Id);

                        if (currentPollingSchedule != null)
                        {
                            _scheduleService.Remove(currentPollingSchedule);
                        }
                        
                        cancellationToken.Cancel();

                        _changeNotifier.Notify();

                        esInstance.Status.UpdateStatus(EventSourceStatusEnum.Stopped);

                        return Task.FromResult(true);
                    };
                }
                else
                {
                    start = (provider, esInstance) =>
                    {
                        var wrapper = _serviceProvider.GetRequiredService<EventSourceActionWrapper>();
                        var wrapped = wrapper.Wrap(action);

                        var jobOptions = new JobOptions { Action = wrapped.Action, ContainsState = wrapped.ContainsState, EventSource = esInstance };

                        _optionsCache.TryAdd(id.ToString(), jobOptions);

                        var schedule = new PollingSchedule(id, pollingFrequency, cronExpression, esInstance);
                        _scheduleService.Add(schedule);

                        // eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Initialized, "Initialized");

                        return Task.FromResult(true);
                    };

                    stop = (provider, esInstance) =>
                    {
                        return Task.FromResult(true);
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to create event source instance from event source {EventSource}", eventSource);
                // eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.InitializingFailed, "Failed: " + e);
            }

            // return eventSourceInstance.Status.Status;

            // if (configurePublisherOptions == null)
            // {
            //     configurePublisherOptions = _optionsMonitor.CurrentValue.ConfigureOptions;
            // }

            var publisherFactoryOptions = new CloudEventPublisherFactoryOptions();

            if (configurePublisherOptions != null)
            {
                publisherFactoryOptions.ConfigureOptions.Add(configurePublisherOptions);
            }
            
            Action<CloudEventCreationOptions> addEventSourceIdConfigurator = creationOptions =>
            {
                creationOptions.AdditionalExtensions = creationOptions.AdditionalExtensions != null
                    ? creationOptions.AdditionalExtensions.Concat(new ICloudEventExtension[]
                    {
                        new EventFrameworkEventSourceExtension(id)
                    }).ToArray()
                    : new ICloudEventExtension[] { new EventFrameworkEventSourceExtension(id) };
            };
            
            publisherFactoryOptions.ConfigureOptions.Add(options =>
            {
                options.ConfigureDefaultCloudEventCreationOptions += addEventSourceIdConfigurator;
            });
            
            _optionsMonitorCache.TryAdd(id.ToString(), publisherFactoryOptions);
            
            var result = new EsInstance(id, eventSource, instanceOptions, start, stop);

            return result;
        }
    }
}

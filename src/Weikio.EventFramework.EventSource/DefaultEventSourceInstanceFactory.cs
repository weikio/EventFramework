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
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.Api;
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
        private readonly ICloudEventPublisherFactory _publisherFactory;
        private readonly IEventSourceDefinitionConfigurationTypeProvider _configurationTypeProvider;
        private readonly IChannelManager _channelManager;
        private readonly IOptionsMonitor<EventSourceOptions> _eventSourceOptionsAccessor;
        private readonly DefaultChannelOptions _defaultChannel;

        public DefaultEventSourceInstanceFactory(IServiceProvider serviceProvider, IOptionsMonitorCache<JobOptions> optionsCache,
            PollingScheduleService scheduleService, ILogger<DefaultEventSourceInstanceFactory> logger, EventSourceChangeNotifier changeNotifier,
            IOptionsMonitorCache<CloudEventPublisherFactoryOptions> optionsMonitorCache, ICloudEventPublisherFactory publisherFactory,
            IEventSourceDefinitionConfigurationTypeProvider configurationTypeProvider,
            IChannelManager channelManager, IOptionsMonitor<EventSourceOptions> eventSourceOptionsAccessor, IOptions<DefaultChannelOptions> defaultChannel)
        {
            _serviceProvider = serviceProvider;
            _optionsCache = optionsCache;
            _scheduleService = scheduleService;
            _logger = logger;
            _changeNotifier = changeNotifier;
            _optionsMonitorCache = optionsMonitorCache;
            _publisherFactory = publisherFactory;
            _configurationTypeProvider = configurationTypeProvider;
            _channelManager = channelManager;
            _eventSourceOptionsAccessor = eventSourceOptionsAccessor;
            _defaultChannel = defaultChannel.Value;
        }

        public EventSourceInstance Create(Abstractions.EventSource eventSource, EventSourceInstanceOptions instanceOptions)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            var eventSourceType = eventSource.EventSourceType;
            var instance = eventSource.Instance;
            var id = instanceOptions.Id ?? Guid.NewGuid().ToString();
            var action = eventSource.Action;
            Func<IServiceProvider, EventSourceInstance, Task<bool>> start = null;
            Func<IServiceProvider, EventSourceInstance, Task<bool>> stop = null;

            var pollingFrequency = instanceOptions.PollingFrequency;
            var cronExpression = instanceOptions.CronExpression;
            var configure = instanceOptions.Configure;

            var eventSourceOptions = _eventSourceOptionsAccessor.CurrentValue;
            var channelName = eventSourceOptions.EventSourceInstanceChannelNameFactory(id);

            var eventSourceInstanceContext = new EventSourceInstanceContext { EventChannel = channelName, EventSourceInstanceId = id };

            try
            {
                _logger.LogInformation("Creating event source instance from event source {EventSource}", eventSource);

                if (eventSourceType == null && instance != null)
                {
                    eventSourceType = instance.GetType();
                }

                var configurationType = _configurationTypeProvider.Get(eventSourceType);

                if (configurationType.RequiresPolling)
                {
                    if (pollingFrequency == null && cronExpression == null)
                    {
                        var optionsManager = _serviceProvider.GetService<IOptionsMonitor<PollingOptions>>();
                        var options = optionsManager.CurrentValue;

                        pollingFrequency = options.PollingFrequency;
                        cronExpression = options.Cron;
                    }
                }

                var isHostedService = eventSourceType != null && typeof(IHostedService).IsAssignableFrom(eventSourceType);
                var isApi = eventSourceType != null && typeof(IApiEventSource).IsAssignableFrom(eventSourceType);
                if (isHostedService)
                {
                    start = HandleHostedService(instanceOptions, eventSourceType, id, eventSourceInstanceContext, configure, out stop);
                }
                else if (isApi)
                {
                    start = HandleApi(instanceOptions, eventSourceType, id, eventSourceInstanceContext, configure, out stop);
                }
                else if (eventSourceType != null)
                {
                    start = HandleType(pollingFrequency, cronExpression, out stop);
                }
                else
                {
                    start = HandleAction(action, id, pollingFrequency, cronExpression, out stop);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create event source instance from event source {EventSource}", eventSource);
            }

            var esChannel = CreateEventSourceInstanceChannel(instanceOptions, channelName, id);

            _channelManager.Add(esChannel);

            var publisherFactoryOptions = new CloudEventPublisherFactoryOptions();

            publisherFactoryOptions.ConfigureOptions.Add(options =>
            {
                options.DefaultChannelName = channelName;
            });

            _optionsMonitorCache.TryAdd(id, publisherFactoryOptions);

            var result = new EventSourceInstance(id, eventSource, instanceOptions, start, stop) { InternalChannelId = channelName };

            return result;
        }

        private Func<IServiceProvider, EventSourceInstance, Task<bool>> HandleAction(MulticastDelegate action, string id, TimeSpan? pollingFrequency, string cronExpression, out Func<IServiceProvider, EventSourceInstance, Task<bool>> stop)
        {
            Func<IServiceProvider, EventSourceInstance, Task<bool>> start;

            start = (provider, esInstance) =>
            {
                var wrapper = _serviceProvider.GetRequiredService<EventSourceActionWrapper>();
                var wrapped = wrapper.Wrap(action);

                var jobOptions = new JobOptions { Action = wrapped.Action, ContainsState = wrapped.ContainsState, EventSource = esInstance };

                _optionsCache.TryAdd(id, jobOptions);

                var schedule = new PollingSchedule(id, pollingFrequency, cronExpression, esInstance);
                _scheduleService.Add(schedule);

                return Task.FromResult(true);
            };

            stop = (provider, esInstance) => { return Task.FromResult(true); };

            return start;
        }

        private Func<IServiceProvider, EventSourceInstance, Task<bool>> HandleType(TimeSpan? pollingFrequency, string cronExpression, out Func<IServiceProvider, EventSourceInstance, Task<bool>> stop)
        {
            Func<IServiceProvider, EventSourceInstance, Task<bool>> start;
            var cancellationToken = new CancellationTokenSource();

            start = (provider, esInstance) =>
            {
                var logger = _serviceProvider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
                var typeToEventSourceTypeProvider = _serviceProvider.GetRequiredService<ITypeToEventSourceTypeProvider>();
                var factory = new TypeToEventSourceFactory(esInstance, logger, _configurationTypeProvider, typeToEventSourceTypeProvider);

                // Event source can contain multiple event sources...

                var sources = factory.Create(_serviceProvider);

                foreach (var eventSourceActionWrapper in sources.PollingEventSources)
                {
                    var childId = eventSourceActionWrapper.Id;

                    var childEventSource = eventSourceActionWrapper.EventSource;

                    var opts = new JobOptions
                    {
                        Action = childEventSource.Action, ContainsState = childEventSource.ContainsState, EventSource = esInstance
                    };
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

            return start;
        }

        private Func<IServiceProvider, EventSourceInstance, Task<bool>> HandleHostedService(EventSourceInstanceOptions instanceOptions, Type eventSourceType, string id,
            EventSourceInstanceContext eventSourceInstanceContext, MulticastDelegate configure, out Func<IServiceProvider, EventSourceInstance, Task<bool>> stop)
        {
            Func<IServiceProvider, EventSourceInstance, Task<bool>> start;
            var cancellationToken = new CancellationTokenSource();

            IHostedService inst = null;

            start = (provider, esInstance) =>
            {
                var extraParams = new List<object>();

                if (instanceOptions.Configuration != null)
                {
                    extraParams.Add(instanceOptions.Configuration);
                }

                if (eventSourceType.GetConstructors().FirstOrDefault()?.GetParameters().Any(x => x.ParameterType == typeof(ICloudEventPublisher)) == true)
                {
                    var publisher = _publisherFactory.CreatePublisher(id);
                    extraParams.Add(publisher);
                }

                if (eventSourceType.GetConstructors().FirstOrDefault()?.GetParameters().Any(x => x.ParameterType == typeof(EventSourceInstanceContext)) == true)
                {
                    extraParams.Add(eventSourceInstanceContext);
                }

                inst = (IHostedService)ActivatorUtilities.CreateInstance(_serviceProvider, eventSourceType, extraParams.ToArray());

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

            return start;
        }
        
          private Func<IServiceProvider, EventSourceInstance, Task<bool>> HandleApi(EventSourceInstanceOptions instanceOptions, Type eventSourceType, string id,
            EventSourceInstanceContext eventSourceInstanceContext, MulticastDelegate configure, out Func<IServiceProvider, EventSourceInstance, Task<bool>> stop)
        {
            Func<IServiceProvider, EventSourceInstance, Task<bool>> start;
            var cancellationToken = new CancellationTokenSource();

            ApiEventSourceRunner inst = null;

            var serviceType = typeof(ApiEventSourceRunner);

            start = (provider, esInstance) =>
            {
                var extraParams = new List<object>();

                if (instanceOptions.Configuration != null)
                {
                    extraParams.Add(instanceOptions.Configuration);
                }

                if (serviceType.GetConstructors().FirstOrDefault()?.GetParameters().Any(x => x.ParameterType == typeof(ICloudEventPublisher)) == true)
                {
                    var publisher = _publisherFactory.CreatePublisher(id);
                    extraParams.Add(publisher);
                }

                if (serviceType.GetConstructors().FirstOrDefault()?.GetParameters().Any(x => x.ParameterType == typeof(EventSourceInstanceContext)) == true)
                {
                    extraParams.Add(eventSourceInstanceContext);
                }

                inst = (ApiEventSourceRunner)ActivatorUtilities.CreateInstance(_serviceProvider, serviceType, extraParams.ToArray());

                if (configure != null)
                {
                    configure.DynamicInvoke(inst);
                }
                
                inst.Initialize(eventSourceType);
                
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

            return start;
        }

        private CloudEventsChannel CreateEventSourceInstanceChannel(EventSourceInstanceOptions instanceOptions, string channelName, string id)
        {
            var channelOptions = new CloudEventsChannelOptions { Name = channelName };

            CloudEventsEndpoint channelEndpoint;

            if (instanceOptions.PublishToChannel && (!string.IsNullOrWhiteSpace(instanceOptions.TargetChannelName) || !string.IsNullOrWhiteSpace(_defaultChannel.DefaultChannelName)))
            {
                channelEndpoint = new CloudEventsEndpoint(async ev =>
                {
                    var channel = _channelManager.Get(string.IsNullOrWhiteSpace(instanceOptions.TargetChannelName) ? _defaultChannel.DefaultChannelName : instanceOptions.TargetChannelName);

                    await channel.Send(ev);
                });
            }
            else
            {
                channelEndpoint = new CloudEventsEndpoint(Task.FromResult);
            }

            channelOptions.CloudEventCreationOptions.AdditionalExtensions = channelOptions.CloudEventCreationOptions.AdditionalExtensions != null
                ? channelOptions.CloudEventCreationOptions.AdditionalExtensions
                    .Concat(new ICloudEventExtension[] { new EventFrameworkEventSourceExtension(id) })
                    .ToArray()
                : new ICloudEventExtension[] { new EventFrameworkEventSourceExtension(id) };

            channelOptions.Endpoints.Add(channelEndpoint);
            instanceOptions.ConfigureChannel?.Invoke(channelOptions);

            var esChannel = new CloudEventsChannel(channelOptions);

            return esChannel;
        }
    }
}

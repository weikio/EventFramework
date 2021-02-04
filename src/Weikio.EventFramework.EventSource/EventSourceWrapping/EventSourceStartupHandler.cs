﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceStartupHandler : IHostedService
    {
        private readonly IEventSourceManager _eventSourceManager;

        public EventSourceStartupHandler(IEventSourceManager eventSourceManager)
        {
            _eventSourceManager = eventSourceManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _eventSourceManager.Update();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    
    public class EventSourceInstanceStartupHandler : IHostedService
    {
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEnumerable<IOptions<EventSourceInstanceOptions>> _initialInstances;
        private readonly ILogger<EventSourceInstanceStartupHandler> _logger;

        public EventSourceInstanceStartupHandler(IEventSourceInstanceManager eventSourceInstanceManager, IEnumerable<IOptions<EventSourceInstanceOptions>> initialInstances, ILogger<EventSourceInstanceStartupHandler> logger)
        {
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _initialInstances = initialInstances;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Creating all the event source instances configured through the IOptions<EventSourceInstanceOptions>");

            var initialInstances = _initialInstances.ToList();

            if (initialInstances.Count < 0)
            {
                _logger.LogDebug("No event source instances created on system startup");

                return;
            }
            
            _logger.LogDebug("Found {InitialInstanceCount} event source instances to create on system startup", initialInstances.Count);

            foreach (var initialInstance in initialInstances)
            {
                await _eventSourceInstanceManager.Create(initialInstance.Value);
            }

            _logger.LogDebug("Created {InitialInstanceCount} event source instances on system startup", initialInstances.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    //
    // /// <summary>
    // /// Types can contain multiple event sources. Because of this a event source action factory is created for each event source.
    // /// This service is run at startup and it "unwraps" the registered event source factories.
    // /// This must be run before QuartzHostedService. 
    // /// </summary>
    // public class EventSourceActionWrapperUnwrapperHost : BackgroundService
    // {
    //     private readonly IEnumerable<TypeToEventSourceFactory> _factories;
    //     private readonly IOptionsMonitorCache<JobOptions> _optionsCache;
    //     private readonly ILogger<EventSourceActionWrapperUnwrapperHost> _logger;
    //     private readonly IServiceProvider _serviceProvider;
    //     private readonly PollingScheduleService _scheduleService;
    //
    //     public EventSourceActionWrapperUnwrapperHost(IEnumerable<TypeToEventSourceFactory> factories, IOptionsMonitorCache<JobOptions> optionsCache,
    //         ILogger<EventSourceActionWrapperUnwrapperHost> logger, IServiceProvider serviceProvider, PollingScheduleService scheduleService)
    //     {
    //         _factories = factories;
    //         _optionsCache = optionsCache;
    //         _logger = logger;
    //         _serviceProvider = serviceProvider;
    //         _scheduleService = scheduleService;
    //     }
    //
    //     protected override Task ExecuteAsync(CancellationToken stoppingToken)
    //     {
    //         var factoryList = _factories.ToList();
    //
    //         if (factoryList.Count == 0)
    //         {
    //             _logger.LogDebug("No event source factories to unwrap. Continue starting event sources.");
    //
    //             return Task.CompletedTask;
    //         }
    //
    //         _logger.LogDebug("Unwrapping {FactoryCount} event source action factories.", factoryList.Count);
    //
    //         try
    //         {
    //             foreach (var wrapperFactory in factoryList)
    //             {
    //                 var originalId = wrapperFactory.Id;
    //                 _logger.LogDebug("Unwrapping factory with {Id}", wrapperFactory.Id);
    //
    //                 var originalSchedule = _scheduleService.Single(x => x.Id == originalId);
    //                 var eventSourceActions = wrapperFactory.Create(_serviceProvider);
    //
    //                 _logger.LogDebug("Found {EventSourceCount} event sources after unwrapping factory with {Id}", eventSourceActions.Count, wrapperFactory.Id);
    //
    //                 foreach (var eventSourceActionWrapper in eventSourceActions)
    //                 {
    //                     var id = eventSourceActionWrapper.Id;
    //
    //                     _logger.LogDebug("Creating event source with {EventSourceID}", id);
    //
    //                     var eventSource = eventSourceActionWrapper.EventSource;
    //                     var opts = new JobOptions { Action = eventSource.Action, ContainsState = eventSource.ContainsState };
    //                     _optionsCache.TryAdd(id, opts);
    //
    //                     var schedule = new PollingSchedule(id, originalSchedule.Interval, originalSchedule.CronExpression);
    //                     _scheduleService.Add(schedule);
    //                 }
    //
    //                 _scheduleService.Remove(originalSchedule);
    //             }
    //
    //             return Task.CompletedTask;
    //         }
    //         catch (Exception e)
    //         {
    //             _logger.LogError(e, "Failed to created event sources from factories.");
    //
    //             throw;
    //         }
    //     }
    // }
}

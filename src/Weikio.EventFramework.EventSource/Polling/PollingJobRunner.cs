using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.EventSource.Polling
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class PollingJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<JobOptions> _optionsMonitor;
        private readonly ILogger<PollingJobRunner> _logger;
        private readonly ICloudEventPublisherFactory _publisherFactory;
        private readonly IEventSourceInstanceManager _instanceManager;

        public PollingJobRunner(IServiceProvider serviceProvider, IOptionsMonitor<JobOptions> optionsMonitor,
            ILogger<PollingJobRunner> logger, 
            ICloudEventPublisherFactory publisherFactory, 
            IEventSourceInstanceManager instanceManager)
        {
            _serviceProvider = serviceProvider;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
            _publisherFactory = publisherFactory;
            _instanceManager = instanceManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var id = context.JobDetail.Key.Name;

                    _logger.LogDebug("Running scheduled event source with {Id}", id);
                    var job = _optionsMonitor.Get(id);

                    if (job.Action == null)
                    {
                        throw new Exception("Unknown event source to run. No action found for event source with id: " + id);
                    }

                    var action = job.Action;
                    var eventSourceId = job.EventSource.Id;

                    var task = (Task) action.DynamicInvoke(new[] { eventSourceId });

                    if (task == null)
                    {
                        throw new Exception("Couldn't execute action for the event source");
                    }

                    await task;

                    _logger.LogDebug("The scheduled event source with {Id} was run successfully", id);

                    EventPollingResult pollingResult = ((dynamic) task).Result;

                    if (pollingResult.IsFirstRun && pollingResult.NewState != null)
                    {
                        _logger.LogDebug("First run done for event source with {Id}. Initialized state to {InitialState}", id, pollingResult.NewState);

                        return;
                    }

                    if (pollingResult.NewState != null)
                    {
                        _logger.LogDebug("Updating the scheduled event source's current state with {Id} to {NewState}", id, pollingResult.NewState);
                    }

                    if (pollingResult.NewEvents?.Any() == true && (!pollingResult.IsFirstRun || pollingResult.NewState == null))
                    {
                        _logger.LogDebug("Publishing new events from event source with {Id}. Event count {EventCount}", id, pollingResult.NewEvents.Count);

                        var publisher = _publisherFactory.CreatePublisher(eventSourceId.ToString());

                        if (pollingResult.NewEvents.Count == 1)
                        {
                            await publisher.Publish(pollingResult.NewEvents.First());
                        }
                        else
                        {
                            await publisher.Publish(pollingResult.NewEvents);
                        }

                        if (job.EventSource.Options.RunOnce)
                        {
                            _logger.LogDebug("Event source instance with id {Id} is configured to run once, auto stopping it", job.EventSource.Id);
                            await _instanceManager.Stop(job.EventSource.Id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run scheduled event source");

                if (e.InnerException != null)
                {
                    _logger.LogError(e.InnerException, "Failed to run scheduled event source, inner exception thrown");
                }

                throw;
            }
        }
    }
}

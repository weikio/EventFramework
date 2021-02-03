using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Weikio.EventFramework.EventPublisher;

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

        public PollingJobRunner(IServiceProvider serviceProvider, IOptionsMonitor<JobOptions> optionsMonitor,
            ILogger<PollingJobRunner> logger, ICloudEventPublisherFactory publisherFactory)
        {
            _serviceProvider = serviceProvider;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
            _publisherFactory = publisherFactory;
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

                    var currentState = context.JobDetail.JobDataMap["state"];
                    var isFirstRun = (bool) context.JobDetail.JobDataMap["isfirstrun"];

                    _logger.LogDebug("Running the scheduled event source with {Id}. Is first run: {IsFirstRun}, current state: {CurrentState}", id,
                        isFirstRun, currentState);

                    var task = (Task) action.DynamicInvoke(new[] { currentState, isFirstRun });

                    if (task == null)
                    {
                        throw new Exception("Couldn't execute action for the event source");
                    }

                    await task;

                    context.JobDetail.JobDataMap["isfirstrun"] = false;

                    _logger.LogDebug("The scheduled event source with {Id} was run successfully", id);

                    EventPollingResult pollingResult = ((dynamic) task).Result;

                    if (isFirstRun && pollingResult.NewState != null)
                    {
                        _logger.LogDebug("First run done for event source with {Id}. Initialized state to {InitialState}.", id, pollingResult.NewState);
                        context.JobDetail.JobDataMap["state"] = pollingResult.NewState;

                        return;
                    }

                    if (pollingResult.NewState != null)
                    {
                        context.JobDetail.JobDataMap["state"] = pollingResult.NewState;
                        _logger.LogDebug("Updating the scheduled event source's current state with {Id} to {NewState}", id, pollingResult.NewState);
                    }

                    if (pollingResult.NewEvents?.Any() == true && (!isFirstRun || pollingResult.NewState == null))
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
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run scheduled event source");

                throw;
            }
        }
    }

    //
    // public class TextFileContentEventSource : IHostedService, IDisposable
    // {
    //     private readonly ILogger<Files.TextFileContentEventSource> _logger;
    //     private readonly ICloudEventPublisher _cloudEventPublisher;
    //     private Timer _timer;
    //     private List<string> _allLines = new List<string>();
    //
    //     // private object _currentState;
    //
    //     public TextFileContentEventSource(ILogger<Files.TextFileContentEventSource> logger, ICloudEventPublisher cloudEventPublisher)
    //     {
    //         _logger = logger;
    //         _cloudEventPublisher = cloudEventPublisher;
    //     }
    //
    //     private void DoWork(object state)
    //     {
    //         var lines = File.ReadLines(@"c:\temp\contentfile.txt").ToList();
    //
    //         var myState = (Files.TextFileContentEventSource.MyState) state;
    //
    //         var currentState = JToken.FromObject(myState.Data);
    //         var newState = JToken.FromObject(lines);
    //
    //         var diff = new JsonDiffPatch();
    //         var res = diff.Diff(currentState, newState);
    //
    //         if (res?.Any() != true)
    //         {
    //             return;
    //         }
    //
    //         if (lines.Count <= _allLines.Count)
    //         {
    //             return;
    //         }
    //
    //         var newLines = lines.Skip(_allLines.Count).ToList();
    //
    //         var result = new NewLinesAddedEvent(newLines);
    //
    //         myState.Data = lines;
    //
    //         // state = lines;
    //
    //         _cloudEventPublisher.Publish(result);
    //     }
    //
    //     public class MyState
    //     {
    //         public object Data { get; set; }
    //     }
    //
    //     public Task StartAsync(CancellationToken stoppingToken)
    //     {
    //         var startingState = File.ReadLines(@"c:\temp\contentfile.txt").ToList();
    //
    //         var myState = new Files.TextFileContentEventSource.MyState() { Data = startingState };
    //
    //         return Task.CompletedTask;
    //     }
    //
    //     public Task StopAsync(CancellationToken stoppingToken)
    //     {
    //         _logger.LogInformation("Timed Hosted Service is stopping.");
    //         _timer?.Change(Timeout.Infinite, 0);
    //
    //         return Task.CompletedTask;
    //     }
    //
    //     public void Dispose()
    //     {
    //         _timer?.Dispose();
    //     }
    // }
}

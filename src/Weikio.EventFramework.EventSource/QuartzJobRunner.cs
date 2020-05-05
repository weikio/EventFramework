using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventPublisher _publisher;
        private readonly IOptionsMonitor<JobOptions> _optionsMonitor;
        private readonly ILogger<QuartzJobRunner> _logger;

        public QuartzJobRunner(IServiceProvider serviceProvider, ICloudEventPublisher publisher, IOptionsMonitor<JobOptions> optionsMonitor, ILogger<QuartzJobRunner> logger)
        {
            _serviceProvider = serviceProvider;
            _publisher = publisher;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var id = Guid.Parse(context.JobDetail.Key.Name);
                
                    _logger.LogDebug("Running scheduled event source with {Id}", id);
                    var job = _optionsMonitor.Get(id.ToString());
                
                    if (job.Action == null)
                    {
                        throw new Exception("Unknown event source to run. No action found for event source with id: " + id);
                    }
                
                    var action = job.Action;

                    Task task;
                    if (job.IsStateless())
                    {
                        task = (Task) action.DynamicInvoke();
                        _logger.LogDebug("Running the scheduled event source with {Id} as stateless", id);
                    }
                    else
                    {
                        var currentState = context.JobDetail.JobDataMap["state"];
                        var isFirstRun = (bool) context.JobDetail.JobDataMap["isfirstrun"];

                        _logger.LogDebug("Running the scheduled event source with {Id} as stateful. Is first run: {IsFirstRun}, current state: {CurrentState}", id, isFirstRun, currentState);
                        
                        task = (Task) action.DynamicInvoke(new[] { currentState, isFirstRun });
                    }

                    if (task == null)
                    {
                        throw new Exception("Couldn't execute action for the event source");
                    }
                
                    await task;

                    context.JobDetail.JobDataMap["isfirstrun"] = false;
                
                    _logger.LogDebug("The scheduled event source with {Id} was run successfully", id);

                    if (job.IsStateless())
                    {
                        return;
                    }
                
                    var actionResult = ((dynamic) task).Result;
                    var updatedState = (object) actionResult;

                    _logger.LogDebug("Updating the scheduled event source's current state with {Id} to {NewState}", id, updatedState);

                    context.JobDetail.JobDataMap["state"] = updatedState;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run scheduled event source");
                throw;
            }
        }


    }

    public static class JobOptionsExtensions
    {
        public static bool IsStateless(this JobOptions jobOptions)
        {
            if (jobOptions == null)
            {
                throw new ArgumentNullException(nameof(jobOptions));
            }

            var action = jobOptions.Action;
            
            return !action.Method.ReturnType.IsGenericType;
        }
        
        public static bool IsStateful(this JobOptions jobOptions)
        {
            return jobOptions.IsStateless() == false;
        }
    }

    public class EventSource
    {
        public EventSource(Func<CloudEvent, Task<bool>> canHandle, Func<CloudEvent, Task> action)
        {
            CanHandle = canHandle;
            Action = action;
        }

        public Func<CloudEvent, Task<bool>> CanHandle { get; set; }
        public Func<CloudEvent, Task> Action { get; set; }
    }

    public class EventPublisherSource
    {
        public EventPublisherSource(Func<Task> action)
        {
            Action = action;
        }

        public Func<Task> Action { get; set; }
    }

    public class NewLinesAddedEvent
    {
        public NewLinesAddedEvent(List<string> newLines)
        {
            NewLines = newLines;
        }

        public List<string> NewLines { get; }
    }

    public class CounterEvent
    {
        public CounterEvent(int count)
        {
            Count = count;
        }

        public int Count { get; }
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

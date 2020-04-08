using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Quartz;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventPublisher _publisher;
        private readonly IOptionsMonitor<JobOptions> _optionsMonitor;

        public QuartzJobRunner(IServiceProvider serviceProvider, ICloudEventPublisher publisher, IOptionsMonitor<JobOptions> optionsMonitor)
        {
            _serviceProvider = serviceProvider;
            _publisher = publisher;
            _optionsMonitor = optionsMonitor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var id = Guid.Parse(context.JobDetail.Key.Name);
                var options = _optionsMonitor.Get(id.ToString());
                if (options.Action == null)
                {
                    throw new Exception("Unknown action to run");
                }
                
                // var jobSchedule = (JobSchedule) context.JobDetail.JobDataMap["schedule"];
                var action = options.Action;

                var currentState = context.JobDetail.JobDataMap["state"];

                var task = (Task) action.DynamicInvoke(new[] { currentState });

                await task;

                if (!task.GetType().IsGenericType)
                {
                    return;
                }

                var updatedState = (object) ((dynamic) task).Result;

                context.JobDetail.JobDataMap["state"] = updatedState;
            }
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

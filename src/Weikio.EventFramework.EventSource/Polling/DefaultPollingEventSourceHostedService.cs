using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class DefaultPollingEventSourceHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly ILogger<DefaultPollingEventSourceHostedService> _logger;
        private readonly IOptionsMonitor<JobOptions> _optionsMonitor;
        private readonly EventSourceChangeProvider _eventSourceChangeProvider;
        private readonly PollingScheduleService _pollingScheduleService;
        private readonly List<IJobDetail> _startedJobs = new List<IJobDetail>();

        public DefaultPollingEventSourceHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory, ILogger<DefaultPollingEventSourceHostedService> logger, PollingScheduleService pollingScheduleService,
            IOptionsMonitor<JobOptions> optionsMonitor, EventSourceChangeProvider eventSourceChangeProvider)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
            _pollingScheduleService = pollingScheduleService;
            _optionsMonitor = optionsMonitor;
            _eventSourceChangeProvider = eventSourceChangeProvider;
            _jobFactory = jobFactory;
        }

        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            _logger.LogInformation("Starting polling event sources. Event source count: {Count}", _pollingScheduleService.Count);

            await StartJobs(cancellationToken);

            _logger.LogInformation("Created {Count} polling event sources. Starting the polling service for the sources", _pollingScheduleService.Count());
            await Scheduler.Start(cancellationToken);
        }

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        
        private async Task StartJobs(CancellationToken cancellationToken)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken);
                
                foreach (var jobSchedule in _pollingScheduleService)
                {
                    try
                    {
                        var job = CreateJob(jobSchedule);
                        var scheduledByQuartz = await Scheduler.GetJobDetail(job.Key, cancellationToken);
                        
                        _logger.LogDebug("Created job with {Id}", jobSchedule.Id);

                        var existingJob = _startedJobs.FirstOrDefault(x => string.Equals(x.Key.Name, jobSchedule.Id, StringComparison.InvariantCultureIgnoreCase));

                        if (existingJob != null || scheduledByQuartz != null)
                        {
                            _logger.LogDebug("Job {JobId} has already started, no need create it again", jobSchedule.Id);

                            if (scheduledByQuartz != null)
                            {
                                _logger.LogDebug("Recreating the trigger for job {JobId} to make sure it is run with correct parameters", jobSchedule.Id);
                                var existingTriggers = await Scheduler.GetTriggersOfJob(scheduledByQuartz.Key, cancellationToken);

                                foreach (var existingTrigger in existingTriggers)
                                {
                                    await Scheduler.UnscheduleJob(existingTrigger.Key, cancellationToken);
                                }

                                await Schedule(cancellationToken, jobSchedule, job);
                            }
                            
                            continue;
                        }
                    
                        _logger.LogDebug("Starting polling event source with {Id}", jobSchedule.Id);
                        await Scheduler.AddJob(job, true, cancellationToken);

                        await Schedule(cancellationToken, jobSchedule, job);

                        _startedJobs.Add(job);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to schedule job");
                    }
                }

                foreach (var startedJob in _startedJobs)
                {
                    var existingJob = _pollingScheduleService.FirstOrDefault(x => string.Equals(x.Id, startedJob.Key.Name));

                    if (existingJob != null)
                    {
                        continue;
                    }
                    
                    _logger.LogDebug("Removing polling event source with {Id}", startedJob.Key.Name);
                    await Scheduler.DeleteJob(startedJob.Key, cancellationToken);
                }
            
                // Listen for changes
                var changeToken = _eventSourceChangeProvider.GetChangeToken();

                changeToken.RegisterChangeCallback(async o =>
                {
                    await StartJobs(cancellationToken);
                }, null);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task Schedule(CancellationToken cancellationToken, PollingSchedule jobSchedule, IJobDetail job)
        {
            var triggers = CreateTriggers(jobSchedule, job);

            foreach (var trigger in triggers)
            {
                await Scheduler.ScheduleJob(trigger, cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(PollingSchedule schedule)
        {
            dynamic jobDetail = JobBuilder
                .Create(typeof(PollingJobRunner))
                .WithIdentity(schedule.Id)
                .WithDescription(schedule.Id)
                .StoreDurably(true)
                .Build();

            return jobDetail;
        }

        private List<ITrigger> CreateTriggers(PollingSchedule schedule, IJobDetail jobDetail)
        {
            if (string.IsNullOrWhiteSpace(schedule.CronExpression) && schedule.Interval == null)
            {
                throw new ArgumentException("Job schedule must include either cron expression or interval");
            }

            var result = new List<ITrigger>();

            var triggerBuilder = TriggerBuilder
                .Create()
                .ForJob(jobDetail)
                .WithIdentity($"{schedule.Id}.trigger")
                .WithDescription(schedule.CronExpression);

            if (!string.IsNullOrWhiteSpace(schedule.CronExpression))
            {
                triggerBuilder = triggerBuilder.WithCronSchedule(schedule.CronExpression);
            }
            else
            {
                triggerBuilder = triggerBuilder.WithSimpleSchedule(x =>
                {
                    x.WithInterval(schedule.Interval.GetValueOrDefault());
                    x.RepeatForever();
                });
            }

            var job = _optionsMonitor.Get(schedule.Id.ToString());

            if (job.ContainsState == false)
            {
                if (string.IsNullOrWhiteSpace(schedule.CronExpression))
                {
                    triggerBuilder = triggerBuilder.StartAt(DateTimeOffset.UtcNow.Add(schedule.Interval.GetValueOrDefault()));
                }

                var trigger = triggerBuilder.Build();
                result.Add(trigger);

                return result;
            }

            var initializationTriggerRequired = !string.IsNullOrWhiteSpace(schedule.CronExpression);

            var cronTrigger = triggerBuilder.Build();
            result.Add(cronTrigger);

            if (initializationTriggerRequired == false)
            {
                return result;
            }

            var initilizationTrigger = TriggerBuilder
                .Create()
                .ForJob(jobDetail)
                .WithIdentity($"{schedule.Id}.initialization.trigger")
                .WithDescription(schedule.CronExpression)
                .StartNow()
                .Build();

            result.Add(initilizationTrigger);

            return result;
        }
    }
}

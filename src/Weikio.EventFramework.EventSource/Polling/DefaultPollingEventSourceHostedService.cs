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
        private readonly PollingScheduleService _pollingScheduleService;

        public DefaultPollingEventSourceHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory, ILogger<DefaultPollingEventSourceHostedService> logger, IOptionsMonitor<JobOptions> optionsMonitor, PollingScheduleService pollingScheduleService)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _pollingScheduleService = pollingScheduleService;
            _jobFactory = jobFactory;
        }

        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;
            
            _logger.LogInformation("Starting polling event sources. Event source count: {Count}", _pollingScheduleService.Count);

            foreach (var jobSchedule in _pollingScheduleService)
            {
                try
                {
                    _logger.LogDebug("Starting polling event source with {Id}", jobSchedule.Id);

                    var job = CreateJob(jobSchedule);

                    await Scheduler.AddJob(job, true, cancellationToken);
                    
                    var triggers = CreateTriggers(jobSchedule, job);

                    foreach (var trigger in triggers)
                    {
                        await Scheduler.ScheduleJob(trigger, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to schedule job");
                }
            }

            _logger.LogInformation("Created {Count} polling event sources. Starting the polling service for the sources", _pollingScheduleService.Count());
            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(PollingSchedule schedule)
        {
            dynamic jobDetail = JobBuilder
                .Create(typeof(PollingJobRunner))
                .WithIdentity(schedule.Id.ToString())
                .UsingJobData("isfirstrun", true)
                .WithDescription(schedule.Id.ToString())
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
                .WithIdentity($"{schedule.Id}.initilization.trigger")
                .WithDescription(schedule.CronExpression)
                .StartNow()
                .Build();
            
            result.Add(initilizationTrigger);
            
            return result;
        }
    }
}

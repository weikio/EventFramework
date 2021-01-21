using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
                        _logger.LogDebug("Created job with {Id}", jobSchedule.Id);

                        var existingJob = _startedJobs.FirstOrDefault(x => string.Equals(x.Key.Name, jobSchedule.Id, StringComparison.InvariantCultureIgnoreCase));

                        if (existingJob != null)
                        {
                            _logger.LogDebug("Job has already started, no need create it again.", jobSchedule.Id);
                            continue;
                        }
                    
                        _logger.LogDebug("Starting polling event source with {Id}", jobSchedule.Id);
                        await Scheduler.AddJob(job, true, cancellationToken);

                        var triggers = CreateTriggers(jobSchedule, job);

                        foreach (var trigger in triggers)
                        {
                            await Scheduler.ScheduleJob(trigger, cancellationToken);
                        }
                    
                        _startedJobs.Add(job);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to schedule job");
                    }
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

    public class EventSourceChangeNotifier
    {
        private readonly EventSourceChangeToken _changeToken;

        public EventSourceChangeNotifier(EventSourceChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        public void Notify()
        {
            _changeToken.TokenSource.Cancel();
        }
    }

    public class EventSourceChangeToken
    {
        public void Initialize()
        {
            TokenSource = new CancellationTokenSource();
        }

        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
    }

    public class EventSourceChangeProvider
    {
        public EventSourceChangeProvider(EventSourceChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        private readonly EventSourceChangeToken _changeToken;

        public IChangeToken GetChangeToken()
        {
            if (_changeToken.TokenSource.IsCancellationRequested)
            {
                _changeToken.Initialize();

                return new CancellationChangeToken(_changeToken.TokenSource.Token);
            }

            return new CancellationChangeToken(_changeToken.TokenSource.Token);
        }
    }
}

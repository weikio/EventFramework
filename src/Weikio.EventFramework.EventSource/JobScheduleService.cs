using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource
{
    public class JobScheduleService : List<JobSchedule>
    {
        private readonly ILogger<JobScheduleService> _logger;

        public JobScheduleService(IEnumerable<JobSchedule> jobSchedules, ILogger<JobScheduleService> logger)
        {
            AddRange(jobSchedules);
            _logger = logger;
        }
    }
}
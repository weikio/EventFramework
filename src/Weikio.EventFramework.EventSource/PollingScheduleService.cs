using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource
{
    public class PollingScheduleService : List<PollingSchedule>
    {
        private readonly ILogger<PollingScheduleService> _logger;

        public PollingScheduleService(IEnumerable<PollingSchedule> jobSchedules, ILogger<PollingScheduleService> logger)
        {
            AddRange(jobSchedules);
            _logger = logger;
        }
    }
}

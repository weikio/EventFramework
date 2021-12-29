using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource.Schedule
{
    [DisplayName("ScheduleEventSource")]
    public class ScheduleEventSource
    {
        private readonly ILogger<ScheduleEventSource> _logger;

        public ScheduleEventSource(ILogger<ScheduleEventSource> logger)
        {
            _logger = logger;
        }
        
        public Task<ScheduleEvent> Run()
        {
            return Task.FromResult(new ScheduleEvent());
        }
    }
}

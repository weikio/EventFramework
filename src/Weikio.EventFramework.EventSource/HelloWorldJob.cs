using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Weikio.EventFramework.EventSource
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class HelloWorldJob : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger;

        public HelloWorldJob(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            context.JobDetail.JobDataMap["hello"] = "test";
            
            if (!context.JobDetail.JobDataMap.ContainsKey("count"))
            {
                context.JobDetail.JobDataMap.Add("count", 0);
            }

            var count = (int) context.JobDetail.JobDataMap["count"];

            count += 1;
            
            _logger.LogInformation("Hello world! " + count);

            context.JobDetail.JobDataMap["count"] = count;
            
            return Task.CompletedTask;
        }
    }
    
    public class HelloWorld2
    {
        private readonly ILogger<HelloWorld2> _logger;

        public HelloWorld2(ILogger<HelloWorld2> logger)
        {
            _logger = logger;
        }

        public Task<int> Execute(int state)
        {
            state += 1;
            _logger.LogInformation("Hello world! " + state);

            return Task.FromResult(state);
        }
    }
}

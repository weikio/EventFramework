using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class DefaultJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public DefaultJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService<PollingJobRunner>();
        }

        public void ReturnJob(IJob job) { }
    }
}

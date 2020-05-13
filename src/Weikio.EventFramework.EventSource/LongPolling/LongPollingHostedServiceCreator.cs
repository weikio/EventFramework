using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public class LongPollingHostedServiceCreator : BackgroundService
    {
        private readonly LongPollingService _longPollingService;
        private readonly IServiceProvider _serviceProvider;

        public LongPollingHostedServiceCreator(LongPollingService longPollingService, IServiceProvider serviceProvider)
        {
            _longPollingService = longPollingService;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var factory in _longPollingService)
            {
                var method = factory.Source;
                var poller = method.Invoke();

                var host = _serviceProvider.GetRequiredService<ILongPollingEventSourceHost>();
                host.Initialize(poller);

                host.StartPolling(stoppingToken);
            }

            return Task.CompletedTask;
        }
    }
}
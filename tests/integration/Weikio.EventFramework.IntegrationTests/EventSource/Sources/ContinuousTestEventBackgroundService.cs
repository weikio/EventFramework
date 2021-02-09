using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("ContinuousTestEventBackgroundService")]
    public class ContinuousTestEventBackgroundService : BackgroundService
    {
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly ContinuousTestEventSourceConfiguration _configuration;

        public ContinuousTestEventBackgroundService(ICloudEventPublisher cloudEventPublisher, ContinuousTestEventSourceConfiguration configuration = null)
        {
            _cloudEventPublisher = cloudEventPublisher;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var originalFiles = new List<string>() { "file1.txt", "file2.txt" };

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                var currentFiles = new List<string>(originalFiles) { Guid.NewGuid().ToString() + ".txt" };

                if (!string.IsNullOrWhiteSpace(_configuration?.ExtraFile))
                {
                    currentFiles.Add(_configuration.ExtraFile);
                }
                
                var result = currentFiles.Except(originalFiles).ToList();

                if (result.Any() == false)
                {
                    continue;
                }

                await _cloudEventPublisher.Publish(result);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}

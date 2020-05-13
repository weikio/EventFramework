using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources
{
    public class ContinuousTestEventBackgroundService : BackgroundService
    {
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public ContinuousTestEventBackgroundService(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var originalFiles = new List<string>() { "file1.txt", "file2.txt" };

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                var currentFiles = new List<string>(originalFiles) { Guid.NewGuid().ToString() + ".txt" };

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
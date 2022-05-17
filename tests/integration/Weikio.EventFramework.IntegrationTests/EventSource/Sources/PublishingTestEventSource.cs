using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    public class PublishingTestEventSource
    {
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public PublishingTestEventSource(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        public async Task<List<string>> CheckForNewFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return files;
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            await _cloudEventPublisher.Publish(newEvents);

            return files;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources
{
    public class TestEventSourceWithMultipleEventTypes
    {
        public async Task<(List<NewFileEvent> NewEvents, List<string> NewState)> CheckForNewFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return (default, files);
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            return (newEvents, files);
        }

        public async Task<(List<DeletedFileEvent> NewEvents, List<string> NewState)> CheckForDeletedFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var newEvents = new List<DeletedFileEvent>();

            if (currentState?.Any() == true)
            {
                var result = files.Except(currentState).ToList();

                foreach (var file in currentState)
                {
                    if (result.Contains(file))
                    {
                        continue;
                    }

                    newEvents.Add(new DeletedFileEvent(file));
                }
            }

            if (newEvents.Any())
            {
                return (newEvents, files);
            }

            return (default, files);
        }
    }
}
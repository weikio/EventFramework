using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    public class TestEventSourceWithMultipleEventTypes
    {
        public Task<(List<NewFileEvent> NewEvents, List<string> NewState)> CheckForNewFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return Task.FromResult<(List<NewFileEvent> NewEvents, List<string> NewState)>((default, files));
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            return Task.FromResult((newEvents, files));
        }

        public Task<(List<DeletedFileEvent> NewEvents, List<string> NewState)> CheckForDeletedFiles(List<string> currentState)
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
                return Task.FromResult((newEvents, files));
            }

            return Task.FromResult<(List<DeletedFileEvent> NewEvents, List<string> NewState)>((default, files));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources
{
    public class TestEventSource
    {
        private readonly string _extraFile;

        public TestEventSource(string extraFile = null)
        {
            _extraFile = extraFile;
        }

        public Task<(List<NewFileEvent> NewEvents, List<string> NewState)> CheckForNewFiles(List<string> currentState)
        {
            List<string> files;

            if (currentState == null)
            {
                files = new List<string>() { "file1.txt", "file2.txt" };
            }
            else
            {
                files = new List<string>() { "file1.txt", "file2.txt", "file3.txt"  };

                if (!string.IsNullOrWhiteSpace(_extraFile))
                {
                    files.Add(_extraFile);
                }
            }

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return Task.FromResult<(List<NewFileEvent> NewEvents, List<string> NewState)>((null, files));
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            return Task.FromResult((newEvents, files));
        }
    }
}

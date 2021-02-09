using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("TestEventSource")]
    public class TestEventSource
    {
        private readonly TestEsConfiguration _configuration;
        private string _extraFile;

        public TestEventSource(TestEsConfiguration configuration = null)
        {
            _configuration = configuration;
        }

        public string ExtraFile
        {
            get
            {
                return _extraFile;
            }
            set
            {
                _extraFile = value;
            }
        }

        public TestEventSource(string extraFile = null)
        {
            ExtraFile = extraFile;
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

                if (!string.IsNullOrWhiteSpace(ExtraFile))
                {
                    files.Add(ExtraFile);
                }

                if (!string.IsNullOrWhiteSpace(_configuration?.ExtraFile))
                {
                    files.Add(_configuration.ExtraFile);
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

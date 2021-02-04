using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources
{
    public class TestEsConfiguration
    {
        public string ExtraFile { get; set; }
    }

    [DisplayName("StatelessTestEventSource")]
    public class StatelessTestEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult(new NewFileEvent("single.txt"));
        }
    }
    
    [DisplayName("StatefulEventSource")]
    public class StatefulEventSource
    {
        private int _runCount = 0;
        public Task<NewFileEvent> Run()
        {
            var newFileEvent = new NewFileEvent($"{_runCount}.txt");

            _runCount += 1;
            return Task.FromResult(newFileEvent);
        }
    }
    
    [DisplayName("MultiEventSource")]
    public class MultiEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult(new NewFileEvent("file.txt"));
        }
        
        public Task<DeletedFileEvent> Deleted()
        {
            return Task.FromResult(new DeletedFileEvent("file.txt"));
        }
    }
    
    [DisplayName("EmptyEventSource")]
    public class EmptyEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult<NewFileEvent>(null);
        }
    }
    
    [DisplayName("StatefulEventSourceWithInitialization")]
    public class StatefulEventSourceWithInitialization
    {
        private int _runCount = 0;
        public Task<NewFileEvent> Run(bool isFirstRun)
        {
            if (isFirstRun)
            {
                _runCount += 10;

                return Task.FromResult<NewFileEvent>(null);
            }
            
            var newFileEvent = new NewFileEvent($"{_runCount}.txt");

            _runCount += 1;
            return Task.FromResult(newFileEvent);
        }
    }
    
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

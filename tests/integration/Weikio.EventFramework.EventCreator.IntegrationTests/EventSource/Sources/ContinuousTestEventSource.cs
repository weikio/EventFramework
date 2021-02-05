using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources
{
    public class ContinuousTestEventSourceConfiguration
    {
        public string ExtraFile { get; set; } = "defaultConfig.txt";
    }

    [DisplayName("MultiTestLongPollerEventSource")]
    public class MultiTestLongPollerEventSource
    {
        public async IAsyncEnumerable<NewFileEvent> CheckForNewFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                var newFileEvent = new NewFileEvent("first.txt");

                yield return newFileEvent;
                
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
        
        public async IAsyncEnumerable<NewFileEvent> CheckForNewFilesSecond([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                var newFileEvent = new NewFileEvent("second.txt");

                yield return newFileEvent;
                
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
    
    [DisplayName("ContinuousTestEventSource")]
    public class ContinuousTestEventSource
    {
        private readonly ContinuousTestEventSourceConfiguration _configuration;

        public ContinuousTestEventSource(ContinuousTestEventSourceConfiguration configuration = null)
        {
            _configuration = configuration;
        }

        public async IAsyncEnumerable<NewFileEvent> CheckForNewFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var originalFiles = new List<string>() { "file1.txt", "file2.txt" };

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
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

                foreach (var res in result)
                {
                    var newFileEvent = new NewFileEvent(res);

                    yield return newFileEvent;
                }

                originalFiles = currentFiles;

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}

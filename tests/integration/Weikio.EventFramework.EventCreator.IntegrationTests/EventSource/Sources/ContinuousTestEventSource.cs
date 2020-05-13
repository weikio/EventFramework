using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventSource.Sources
{
    public class ContinuousTestEventSource
    {
        public async IAsyncEnumerable<NewFileEvent> CheckForNewFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var originalFiles = new List<string>() { "file1.txt", "file2.txt" };

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                var currentFiles = new List<string>(originalFiles) { Guid.NewGuid().ToString() + ".txt" };

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
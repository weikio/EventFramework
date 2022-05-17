using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
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
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Samples.EventSource
{
    [DisplayName("TestEventSource")]
    public class TestEventSource
    {

        public Task<(List<NewFileEvent> NewEvents, List<string> NewState)> CheckForNewFiles(List<string> currentState)
        {
            List<string> files;

            if (currentState == null)
            {
                files = new List<string>() { "file1.txt", "file2.txt" };
            }
            else
            {
                files = new List<string>(currentState) { DateTime.Now.ToString(CultureInfo.InvariantCulture)+".txt" };
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

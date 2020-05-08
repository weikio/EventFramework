using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource
{
    public class NewLinesAddedEvent
    {
        public NewLinesAddedEvent(List<string> newLines)
        {
            NewLines = newLines;
        }

        public List<string> NewLines { get; }
    }
}
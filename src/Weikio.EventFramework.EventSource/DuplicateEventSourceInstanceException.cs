using System;

namespace Weikio.EventFramework.EventSource
{
    public class DuplicateEventSourceInstanceException : Exception
    {
        public DuplicateEventSourceInstanceException(string message) : base(message)
        {
        }
    }
}

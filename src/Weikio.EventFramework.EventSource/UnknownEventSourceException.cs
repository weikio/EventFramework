using System;

namespace Weikio.EventFramework.EventSource
{
    public class UnknownEventSourceException : Exception
    {
        public UnknownEventSourceException(string message) : base(message)
        {
        }
    }
}

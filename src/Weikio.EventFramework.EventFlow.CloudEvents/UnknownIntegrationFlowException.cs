using System;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class UnknownEventFlowException : Exception
    {
        public UnknownEventFlowException()
        {
        }

        public UnknownEventFlowException(string message) : base(message)
        {
        }
    }
}
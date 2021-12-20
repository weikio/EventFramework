using System;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class UnknownEventFlowInstance : Exception
    {
        public string Id { get; }

        public UnknownEventFlowInstance(string id, string message) : base(message)
        {
            Id = id;
        }
    }
}
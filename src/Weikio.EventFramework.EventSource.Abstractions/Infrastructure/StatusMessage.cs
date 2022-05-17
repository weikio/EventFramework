using System;

namespace Weikio.EventFramework.EventSource.Abstractions.Infrastructure
{
    public class StatusMessage
    {
        public DateTime MessageTime { get; }
        public string Message { get; }

        public StatusMessage(DateTime messageTime, string message)
        {
            MessageTime = messageTime;
            Message = message;
        }
    }
}

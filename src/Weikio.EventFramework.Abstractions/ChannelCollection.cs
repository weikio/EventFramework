using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Weikio.EventFramework.Abstractions
{
    public class ChannelCollection : List<IChannel>, IChannelCollection
    {
        public IEnumerable<IChannel> Channels => this;
        
        public IChannel Get(string channelName)
        {
            var result = this.FirstOrDefault(x => string.Equals(channelName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result == null)
            {
                throw new UnknownChannelException(channelName);
            }

            return result;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EventDescriptionAttribute : Attribute
    {
        public string EventType { get; }
        public string Subject { get; }

        public EventDescriptionAttribute(string eventType = null, string subject = null)
        {
            EventType = eventType;
            Subject = subject;
        }
    }
}

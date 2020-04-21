using System;

namespace Weikio.EventFramework.EventGateway
{
    public class UnknownChannelException : Exception
    {
        public UnknownChannelException(string channelName)
        {
            ChannelName = channelName;
        }

        public string ChannelName { get; }
    }
}
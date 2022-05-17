using System;

namespace Weikio.EventFramework.Channels
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
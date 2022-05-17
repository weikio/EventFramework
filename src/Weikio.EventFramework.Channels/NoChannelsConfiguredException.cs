using System;

namespace Weikio.EventFramework.Channels
{
    public class NoChannelsConfiguredException : Exception
    {
        public NoChannelsConfiguredException() : base("Tried to get channel but there is no channels configured. Make sure to add one into your system.")
        {
        }
    }
    
    public class NoDefaultChannelFoundException : Exception
    {
        public NoDefaultChannelFoundException() : base("Tried to get the default channel but there is no default channel configured. Make sure to add one into your system by configuring DefaultChannelOptions.")
        {
        }
    }
}

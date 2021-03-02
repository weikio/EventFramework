using System;

namespace Weikio.EventFramework.Channels
{
    public class NoChannelsConfiguredException : Exception
    {
        public NoChannelsConfiguredException() : base("Tried to get channel but there is not channels configured. Make sure to add one into your system.")
        {
        }
    }
}
﻿using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelFactory
    {
        IChannel Create();
    }
}

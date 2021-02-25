﻿using System;

namespace Weikio.EventFramework.EventGateway
{
    public class NoGatewaysConfiguredException : Exception
    {
        public NoGatewaysConfiguredException() : base("Tried to publish event to gateway but there is not gateways configured. Make sure to add one into your system using services.AddGateway()")
        {
        }
    }
    
    public class NoChannelsConfiguredException : Exception
    {
        public NoChannelsConfiguredException() : base("Tried to get channel but there is not channels configured. Make sure to add one into your system.")
        {
        }
    }
}

using System;

namespace Weikio.EventFramework.Abstractions
{
    public class UnknownGatewayException : Exception
    {
        public UnknownGatewayException(string gatewayName)
        {
            GatewayName = gatewayName;
        }

        public string GatewayName { get; }
    }
}
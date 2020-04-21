using System;

namespace Weikio.EventFramework.EventGateway
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
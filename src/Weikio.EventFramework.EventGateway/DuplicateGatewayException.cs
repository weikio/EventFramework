using System;

namespace Weikio.EventFramework.EventGateway
{
    public class DuplicateGatewayException : Exception
    {
        public DuplicateGatewayException(string gatewayName)
        {
            GatewayName = gatewayName;
        }

        public string GatewayName { get; }
    }
}
using System;

namespace Weikio.EventFramework.Abstractions
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
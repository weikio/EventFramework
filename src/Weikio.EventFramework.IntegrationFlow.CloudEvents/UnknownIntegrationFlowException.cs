using System;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class UnknownIntegrationFlowException : Exception
    {
        public UnknownIntegrationFlowException()
        {
        }

        public UnknownIntegrationFlowException(string message) : base(message)
        {
        }
    }
}
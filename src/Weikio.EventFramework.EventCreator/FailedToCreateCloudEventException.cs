using System;

namespace Weikio.EventFramework.EventCreator
{
    public class FailedToCreateCloudEventException : Exception
    {
        public FailedToCreateCloudEventException(Exception innerException, string message = null) : base(message, innerException)
        {
        }
    }
}

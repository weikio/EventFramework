using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.Components.Logger
{
    public class LoggerEndpointOptions
    {
        /// <summary>
        /// The LogLevel which is used to log events
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}

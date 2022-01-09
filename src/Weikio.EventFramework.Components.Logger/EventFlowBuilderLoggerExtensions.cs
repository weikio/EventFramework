using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Components.Logger;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderLoggerExtensions
    {
        public static IEventFlowBuilder Logger(this IEventFlowBuilder builder, LogLevel logLevel = LogLevel.Information)
        {
            var loggerOptions = new LoggerEndpointOptions() { LogLevel = logLevel };

            var componentBuilder = new LoggerEndpoint(loggerOptions);

            builder.Component(componentBuilder);

            return builder;
        }
    }
}

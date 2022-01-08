using System;
using System.Net.Http;
using System.Threading.Tasks;
using Weikio.EventFramework.Components.File;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderFileExtensions
    {
        public static IEventFlowBuilder File(this IEventFlowBuilder builder, string folder, Action<FileEndpointOptions> configureOptions = null)
        {
            var fileEndpointOptions = new FileEndpointOptions()
            {
                Folder = folder
            };

            configureOptions?.Invoke(fileEndpointOptions);

            var componentBuilder = new FileEndpoint(fileEndpointOptions);

            builder.Component(componentBuilder);

            return builder;
        }
    }
}

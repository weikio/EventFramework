using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class DefaultEventFlowInstanceFactory : IEventFlowInstanceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultEventFlowInstanceFactory> _logger;
        private readonly IOptions<EventFlowChannelDefaultComponents> _options;

        public DefaultEventFlowInstanceFactory(IServiceProvider serviceProvider, ILogger<DefaultEventFlowInstanceFactory> logger, IOptions<EventFlowChannelDefaultComponents> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
        }

        public async Task<EventFlowInstance> Create(EventFlow eventFlow, EventFlowInstanceOptions options)
        {
            // Component channel names are created based on the index, so 0, 1, 2 etc.
            // Is there any use for the fact that we use index numbers? Maybe just give Guid to the component and use that as channel name.
            // Or, why we create the components at this point? Why doesn't the DefaultCloudEventsIntegrationFlowManager create them?
            // Or maybe the channels should be created here
            for (var index = 0; index < eventFlow.ComponentFactories.Count; index++)
            {
                var componentFactory = eventFlow.ComponentFactories[index];
                
                // TODO: Find a place for this
                var componentChannelName = $"system/flows/{options.Id}/componentchannels/{eventFlow.Components.Count}";

                var hasNextComponent = eventFlow.ComponentFactories.Count > index + 1;

                string nextComponentChannelName = null;

                if (hasNextComponent)
                {
                    // TODO: Find a place for this
                    nextComponentChannelName = $"system/flows/{options.Id}/componentchannels/{ eventFlow.Components.Count + 1}";
                }
                
                var context = new ComponentFactoryContext(_serviceProvider, 
                    eventFlow, 
                    options, 
                    eventFlow.Components.Count, 
                    componentChannelName, 
                    nextComponentChannelName);

                var component = await componentFactory(context);
                eventFlow.Components.Add(component);
            }

            // Insert an endpoint which transfer the event to the desired channel if event has attribute eventFramework_eventFlow_endpoint
            eventFlow.Endpoints.Add(new CloudEventsEndpoint(async ev =>
            {
                var attrs = ev.GetAttributes();

                if (attrs.ContainsKey(EventFrameworkEventFlowEndpointEventExtension.EventFrameworkEventFlowEndpointAttributeName) == false)
                {
                    return;
                }

                var targetChannel = attrs[EventFrameworkEventFlowEndpointEventExtension.EventFrameworkEventFlowEndpointAttributeName] as string;

                if (string.IsNullOrWhiteSpace(targetChannel))
                {
                    return;
                }

                var channel = _serviceProvider.GetRequiredService<IChannelManager>().Get(targetChannel);
                
                // After a transfer, we want to remove the attribute so that the event doesn't get stuck in a loop
                attrs.Remove(EventFrameworkEventFlowEndpointEventExtension.EventFrameworkEventFlowEndpointAttributeName);
                
                await channel.Send(ev);
            }));

            var result = new EventFlowInstance(eventFlow, options);

            return result;
        }
    }
}

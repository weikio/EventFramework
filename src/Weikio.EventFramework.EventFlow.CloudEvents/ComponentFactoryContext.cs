using System;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public EventFlow EventFlow { get; }
        public EventFlowInstanceOptions Options { get; }
        public int CurrentComponentIndex { get; }
        public string CurrentComponentChannelName { get; set; }
        public string NextComponentChannelName { get; set; }

        public ComponentFactoryContext(IServiceProvider serviceProvider, EventFlow eventFlow, EventFlowInstanceOptions options,
            int currentComponentIndex, string currentComponentChannelName, string nextComponentChannelName)
        {
            ServiceProvider = serviceProvider;
            EventFlow = eventFlow;
            Options = options;
            CurrentComponentIndex = currentComponentIndex;
            CurrentComponentChannelName = currentComponentChannelName;
            NextComponentChannelName = nextComponentChannelName;
        }
    }
}

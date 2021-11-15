using Weikio.EventFramework.IntegrationFlow.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class ConfigurationFlow : CloudEventsIntegrationFlowBase
    {
        public ConfigurationFlow(Config config)
        {
            Flow = IntegrationFlowBuilder.From("local")
                .Channel(config.TargetChannelName);
        }

        public class Config
        {
            public string TargetChannelName { get; set; }
        }
    }
    
    public class InterceptorFlow : CloudEventsIntegrationFlowBase
    {
        public InterceptorFlow()
        {
            Flow = IntegrationFlowBuilder.From("local")
                .Transform(ev => ev)
                .Transform(ev => ev)
                .Channel("intercepted");
        }
    }
}

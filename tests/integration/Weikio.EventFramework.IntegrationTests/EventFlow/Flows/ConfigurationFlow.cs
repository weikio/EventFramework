using Weikio.EventFramework.EventFlow.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.EventFlow.Flows
{
    public class ConfigurationFlow : CloudEventFlowBase
    {
        public ConfigurationFlow(Config config)
        {
            Flow = EventFlowBuilder.From("local")
                .Channel(config.TargetChannelName);
        }

        public class Config
        {
            public string TargetChannelName { get; set; }
        }
    }
    
    public class InterceptorFlow : CloudEventFlowBase
    {
        public InterceptorFlow()
        {
            Flow = EventFlowBuilder.From("local")
                .Transform(ev => ev)
                .Transform(ev => ev)
                .Channel("intercepted");
        }
    }
}

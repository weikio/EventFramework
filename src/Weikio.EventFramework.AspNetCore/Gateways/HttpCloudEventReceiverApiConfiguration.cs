namespace Weikio.EventFramework.AspNetCore.Gateways
{
    public class HttpCloudEventReceiverApiConfiguration
    {
        public string GatewayName { get; set; }
        public string InputChannelName { get; set; }
        public string PolicyName { get; set; }
    }
}
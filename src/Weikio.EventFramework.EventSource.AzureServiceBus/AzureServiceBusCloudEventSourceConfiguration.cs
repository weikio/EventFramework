namespace Weikio.EventFramework.EventSource.AzureServiceBus
{
    public class AzureServiceBusCloudEventSourceConfiguration
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}

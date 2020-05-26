using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public static class CloudEventExtensions 
    {
        public static string Gateway(this CloudEvent cloudEvent)
        {
            if (cloudEvent?.GetAttributes()?.ContainsKey(EventFrameworkCloudEventExtension.EventFrameworkIncomingGatewayAttributeName) == true)
            {
                return cloudEvent.GetAttributes()[EventFrameworkCloudEventExtension.EventFrameworkIncomingGatewayAttributeName] as string;
            }

            return null;
        }
        
        public static string Channel(this CloudEvent cloudEvent)
        {
            if (cloudEvent?.GetAttributes()?.ContainsKey(EventFrameworkCloudEventExtension.EventFrameworkIncomingChannelAttributeName) == true)
            {
                return cloudEvent.GetAttributes()[EventFrameworkCloudEventExtension.EventFrameworkIncomingChannelAttributeName] as string;
            }

            return null;
        }
        

    }
}

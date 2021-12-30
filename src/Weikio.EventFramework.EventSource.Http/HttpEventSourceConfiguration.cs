namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpEventSourceConfiguration
    {
        public string Endpoint { get; set; } = "/events";
        public string PolicyName { get; set; }
    }
}

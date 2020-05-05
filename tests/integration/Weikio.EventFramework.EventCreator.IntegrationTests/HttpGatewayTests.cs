using System.Net.Http;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventGateway.Http;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests
{
    public class HttpGatewayTests :  EventFrameworkTestBase
    {
        public HttpGatewayTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CanCreateHttpGateway()
        {
            var server = Init(services =>
            {
                services.AddHttpGateway();
            });
            
            var json = "{\n    \"specversion\" : \"1.0\",\n    \"type\" : \"new-file\",\n    \"source\" : \"https://github.com/cloudevents/spec/pull\",\n    \"subject\" : \"123\",\n    \"id\" : \"A234-1234-1234\",\n    \"time\" : \"2018-04-05T17:31:00Z\",\n    \"comexampleextension1\" : \"value\",\n    \"comexampleothervalue\" : 5,\n    \"datacontenttype\" : \"text/xml\",\n    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n}";

            // Assert & Act: Does not throw
            await server.PostAsync("/api/events", new StringContent(json));
        }
        
        [Fact]
        public async Task CanCreateHttpGatewayWithCustomUrl()
        {
            var server = Init(services =>
            {
                services.AddHttpGateway(endpoint: "/test/myEvents");
            });
            
            var json = "{\n    \"specversion\" : \"1.0\",\n    \"type\" : \"new-file\",\n    \"source\" : \"https://github.com/cloudevents/spec/pull\",\n    \"subject\" : \"123\",\n    \"id\" : \"A234-1234-1234\",\n    \"time\" : \"2018-04-05T17:31:00Z\",\n    \"comexampleextension1\" : \"value\",\n    \"comexampleothervalue\" : 5,\n    \"datacontenttype\" : \"text/xml\",\n    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n}";

            // Assert & Act: Does not throw
            await server.PostAsync("/test/myEvents", new StringContent(json));
        }
        
        [Fact]
        public async Task CanCreateMultipleHttpGateways()
        {
            var server = Init(services =>
            {
                services.AddHttpGateway("web1", "/api/first");
                services.AddHttpGateway("web2", "/api/second");
            });
            
            var json = "{\n    \"specversion\" : \"1.0\",\n    \"type\" : \"new-file\",\n    \"source\" : \"https://github.com/cloudevents/spec/pull\",\n    \"subject\" : \"123\",\n    \"id\" : \"A234-1234-1234\",\n    \"time\" : \"2018-04-05T17:31:00Z\",\n    \"comexampleextension1\" : \"value\",\n    \"comexampleothervalue\" : 5,\n    \"datacontenttype\" : \"text/xml\",\n    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n}";

            // Assert & Act: Does not throw
            await server.PostAsync("/api/first", new StringContent(json));
            await server.PostAsync("/api/second", new StringContent(json));
        }
    }
}

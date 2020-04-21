using System.Net.Http;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.Router;
using Xunit;

namespace Weikio.EventFramework.EventCreator.IntegrationTests
{
    public class RouterTests : EventFrameworkTestBase
    {
        public RouterTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CanRoute()
        {
            var server = Init(services =>
            {
                services.AddHttpGateway("web1", "/api/first");
                services.AddHttpGateway("web2", "/api/second", "/router/endpoint");

                services.AddRoute("web1", "web2");
            });
            
            var json = "{\n    \"specversion\" : \"1.0\",\n    \"type\" : \"new-file\",\n    \"source\" : \"https://github.com/cloudevents/spec/pull\",\n    \"subject\" : \"123\",\n    \"id\" : \"A234-1234-1234\",\n    \"time\" : \"2018-04-05T17:31:00Z\",\n    \"comexampleextension1\" : \"value\",\n    \"comexampleothervalue\" : 5,\n    \"datacontenttype\" : \"text/xml\",\n    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n}";

            // Act
            await server.PostAsync("/api/first", new StringContent(json));

            var count = await server.GetJsonAsync<int>("/router/endpoint");
            
            Assert.Equal(1, count);
        }
    }
}

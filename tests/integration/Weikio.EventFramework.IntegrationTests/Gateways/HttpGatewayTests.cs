using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.Gateways
{
    public class ChannelTests : EventFrameworkTestBase
    {
        public ChannelTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task DataflowsWork()
        {
            Init(services =>
            {
                services.AddCloudEventGateway();
                services.AddLocal();
            });

            var gwManager = ServiceProvider.GetRequiredService<ICloudEventChannelManager>();
            var gw = new CloudEventGateway("test", null, new DataflowChannel(gwManager, "test", "local"));

            var c = gw.OutgoingChannel;
            await c.Send(CloudEventCreator.Create(new CustomerCreatedEvent()));
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    public class HttpGatewayTests :  EventFrameworkTestBase
    {
        public HttpGatewayTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
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
            var res = await server.PostAsync("/api/events", new StringContent(json));
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
        
        [Fact]
        public async Task CanCreateHttpGatewayWithCustomUrl()
        {
            var server = Init(services =>
            {
                services.AddHttpGateway(endpoint: "/myEvents");
            });
            
            var json = "{\n    \"specversion\" : \"1.0\",\n    \"type\" : \"new-file\",\n    \"source\" : \"https://github.com/cloudevents/spec/pull\",\n    \"subject\" : \"123\",\n    \"id\" : \"A234-1234-1234\",\n    \"time\" : \"2018-04-05T17:31:00Z\",\n    \"comexampleextension1\" : \"value\",\n    \"comexampleothervalue\" : 5,\n    \"datacontenttype\" : \"text/xml\",\n    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n}";

            // Assert & Act: Does not throw
            var res = await server.PostAsync("/api/myEvents", new StringContent(json));
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
        
        [Fact]
        public async Task CanCreateMultipleHttpGateways()
        {
            var server = Init(services =>
            {
                services.AddHttpGateway("web1", "/first");
                services.AddHttpGateway("web2", "/second");
            });
            
            var json = "{\n    \"specversion\" : \"1.0\",\n    \"type\" : \"new-file\",\n    \"source\" : \"https://github.com/cloudevents/spec/pull\",\n    \"subject\" : \"123\",\n    \"id\" : \"A234-1234-1234\",\n    \"time\" : \"2018-04-05T17:31:00Z\",\n    \"comexampleextension1\" : \"value\",\n    \"comexampleothervalue\" : 5,\n    \"datacontenttype\" : \"text/xml\",\n    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n}";

            // Assert & Act: Does not throw
            var res1= await server.PostAsync("/api/first", new StringContent(json));
            var res2=await server.PostAsync("/api/second", new StringContent(json));
            
            Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        }
        
        [Fact]
        public void HttpGatewayIsHttpPost()
        {
            var server = InitFactory(services =>
            {
                services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());
                
                services.AddHttpGateway("web1", "/first");
            });


            var apiEpxplorer = server.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            var gatewayGroup = apiEpxplorer.ApiDescriptionGroups.Items.Single(x => string.Equals(x.GroupName, "gateway", StringComparison.InvariantCultureIgnoreCase));

            var api = gatewayGroup.Items.Single();
            Assert.Equal(HttpMethods.Post, api.HttpMethod);
        }
    }
}

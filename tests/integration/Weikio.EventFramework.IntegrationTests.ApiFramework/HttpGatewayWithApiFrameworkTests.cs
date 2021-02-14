using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.AspNetCore;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.Extensions.EventAggregator;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.ApiFramework
{
    public class HttpGatewayWithApiFrameworkTests : EventFrameworkTestBase
    {
        
        public HttpGatewayWithApiFrameworkTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }
        
        [Fact]
        public async Task ApiFrameworkAndHttpGatewayWorkTogether()
        {
            Init(services =>
            {
                services.AddApiFramework()
                    .AddApi<TestApi>("/hello");
                
                services.AddEventFramework()
                    .AddHttpGateway()
                    .AddHandler<TestHandler>();
            });

            
            var ev = CloudEventCreator.CreateJson(new TestEvent());
            var evContent = new StringContent(ev, Encoding.UTF8, "application/cloudevents+json");

            await Task.Delay(TimeSpan.FromSeconds(1));
            await ContinueWhen(async () =>
            {
                var res = await Client.PostAsync("/api/api/events", evContent);
            
                return TestHandler.Handled;
            }, timeout: TimeSpan.FromSeconds(5));
            
            await ContinueWhen(async () =>
            {
                var res  = await Client.GetStringAsync("/api/hello");

                return res == "world";
            });
        }
    }

    public class TestEvent
    {
        public string Test { get; set; } = "For the handler";
    }

    public class TestHandler
    {
        public static bool Handled { get; set; }
        
        public Task Handle(CloudEvent cloudEvent)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }

    public class TestApi
    {
        public string GetHello()
        {
            return "world";
        }
    }
}

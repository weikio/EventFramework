using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;
using Weikio.EventFramework.IntegrationTests.EventFlow.Flows;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class CloudEventFlowInterceptorTests : IntegrationFlowTestBase
    {
        public CloudEventFlowInterceptorTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanAddInterceptorBeforeEachStep()
        {
            var counterInterceptor = new CounterInterceptor();
            var counter = new Counter();

            var provider = Init(services =>
            {
                services.AddChannel("local");
                services.AddChannel("intercepted", (serviceProvider, options) =>
                {
                    options.Endpoint = ev =>
                    {
                        counter.Increment();
                    };
                });

                services.AddEventFlow<InterceptorFlow>(flow =>
                {
                    flow.Flow.WithInterceptor(InterceptorTypeEnum.PreComponent, counterInterceptor);
                });
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");

            await channel.Send(new CustomerCreatedEvent());
            await ContinueWhen(() => counter.Get() > 0, timeout: TimeSpan.FromSeconds(5));

            // The automatically added EventFrameworkIntegrationFlowEventExtension counts as one
            Assert.Equal(6, counterInterceptor.Counter);
        }
    }
}

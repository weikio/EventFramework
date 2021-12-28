using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.Channels
{
    public class ChannelTests : ChannelTestBase
    {
        public ChannelTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanCreateChannelUsingBuilder()
        {
            var server = Init();

            var channel = CloudEventsChannelBuilder.From()
                .WithName("testchannel")
                .Component(new FilterComponentBuilder(ev =>
                {
                    if (string.Equals(ev.Type, "CustomerCreatedEvent"))
                    {
                        return Filter.Continue;
                    }

                    return Filter.Skip;
                }).Build)
                .Build(server);
        }
        
        [Fact]
        public async Task ChannelEndpointsAreNotDuplicated()
        {
            var count = 0;
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddLocal()
                    .AddEventSource<StatelessTestEventSource>(options =>
                    {
                        options.Autostart = true;
                        options.TargetChannelName = "bus";
                        options.PollingFrequency = TimeSpan.FromSeconds(30);
                    })
                    .AddChannel("bus", (provider, options) =>
                    {
                        options.Endpoints.Add((ev =>
                        {
                            count += 1;
                            return Task.CompletedTask;
                        }, null));
                    })
                    .AddChannel("bus2", (provider, options) =>
                    {
                        options.Endpoints.Add((ev =>
                        {
                            count += 1;
                            return Task.CompletedTask;
                        }, null));
                    });

                services.Configure<CloudEventPublisherOptions>(options =>
                {
                    options.DefaultChannelName = "bus2";
                });
                
                services.AddTransient<ICloudEventPublisherBuilder, TestCloudEventPublisherBuilder>();
            });

            var publisher = server.GetRequiredService<ICloudEventPublisher>();

            await publisher.Publish(CloudEventCreator.Create(new CustomerCreatedEvent()));

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(1, count);
        }
    }
}

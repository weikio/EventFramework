using System.Net.Http;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Extensions.EventAggregator;
using Weikio.EventFramework.IntegrationTests.EventSource;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests
{
    public class ChannelDemo : ChannelTestBase
    {
        public ChannelDemo(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task NewChannel()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("local");
            });

            var channelManager = server.GetRequiredService<IChannelManager>();
            var channel = channelManager.Get("local");

            await channel.Send(new CustomerCreatedEvent() { Name = "mk" });
        }

        [Fact]
        public async Task EventPublisher()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("local");
            });

            var publisher = server.GetRequiredService<ICloudEventPublisher>();
            await publisher.Publish(new CustomerCreatedEvent(), "local");
        }

        [Fact]
        public async Task EventPublisherFactory()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("local");
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task ChannelEndpoint()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("local", (provider, options) =>
                    {
                        var logger = provider.GetRequiredService<ILogger<ChannelDemo>>();

                        options.Endpoint = ev =>
                        {
                            logger.LogInformation(ev.ToJson());
                        };
                    });
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task MultipleChannelEndpoints()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("local", (provider, options) =>
                    {
                        var logger = provider.GetRequiredService<ILogger<ChannelDemo>>();

                        options.Endpoint = ev =>
                        {
                            logger.LogInformation(ev.ToJson());
                        };

                        var httpClient = new HttpClient();

                        options.Endpoints.Add((async ev =>
                        {
                            await httpClient.PostJsonAsync("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874", ev);
                        }, null));
                    });
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task ChannelBuilder()
        {
            var channel = CloudEventsChannelBuilder.From("local")
                .Logger()
                .Http("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874");

            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(channel);
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task BackgroundProcessing()
        {
            var channel = CloudEventsChannelBuilder.From("local")
                .EventAggregator();

            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(channel)
                    .AddHandler<Handler>();
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task MultipleChannels()
        {
            var channel = CloudEventsChannelBuilder.From("local")
                .EventAggregator()
                .Channel("web");

            var webChannel = CloudEventsChannelBuilder.From("web")
                .Http("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874");

            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(channel)
                    .AddChannel(webChannel)
                    .AddHandler<Handler>();
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task Subscribe()
        {
            var channel = CloudEventsChannelBuilder.From("local")
                .EventAggregator();

            var webChannel = CloudEventsChannelBuilder.From("web")
                .Subscribe("local")
                .Http("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874");

            var diagChannel = CloudEventsChannelBuilder.From("diag")
                .Subscribe("local")
                .Subscribe("web")
                .Logger();

            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(channel)
                    .AddChannel(webChannel)
                    .AddChannel(diagChannel)
                    .AddHandler<Handler>();
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        [Fact]
        public async Task Crypt()
        {
            var channel = CloudEventsChannelBuilder.From("local")
                .EventAggregator();

            var webChannel = CloudEventsChannelBuilder.From("web")
                .Subscribe("local")
                .Encrypt(publicKeyPath: @"c:\temp\public.key")
                .Http("https://webhook.site/3bdf5c39-065b-48f8-8356-511b284de874");

            var diagChannel = CloudEventsChannelBuilder.From("diag")
                .Subscribe("local")
                .Subscribe("web")
                .Logger();

            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(channel)
                    .AddChannel(webChannel)
                    .AddChannel(diagChannel)
                    .AddHandler<Handler>();
            });

            var factory = server.GetRequiredService<ICloudEventPublisherFactory>();
            var publisher = factory.CreatePublisher("local");

            await publisher.Publish(new CustomerCreatedEvent());
        }

        public class Handler
        {
            private readonly ILogger<Handler> _logger;

            public Handler(ILogger<Handler> logger)
            {
                _logger = logger;
            }

            public Task Handle(CustomerCreatedEvent ev)
            {
                return Task.CompletedTask;
            }
        }
    }
}

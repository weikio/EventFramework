using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.AzureServiceBus;
using Weikio.EventFramework.EventSource.Http;
using Weikio.EventFramework.Extensions.EventAggregator;
using Weikio.EventFramework.IntegrationTests.EventSource;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests
{
    public class EventSourceDemo : ChannelTestBase
    {
        public EventSourceDemo(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task NewEventSource()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddEventSource<AzureServiceBusCloudEventSource>();
            });

            var esManager = server.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "AzureServiceBusCloudEventSource",
                Configuration = new AzureServiceBusCloudEventSourceConfiguration()
                {
                    QueueName = "sqlexecutor",
                    ConnectionString =
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4="
                },
                Autostart = true,
            };

            var esInstance = await esManager.Create(options);
            await esManager.Start(esInstance);

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task EventSourceChannel()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddEventSource<AzureServiceBusCloudEventSource>();
            });

            var esManager = server.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "AzureServiceBusCloudEventSource",
                Configuration = new AzureServiceBusCloudEventSourceConfiguration()
                {
                    QueueName = "sqlexecutor",
                    ConnectionString =
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4="
                },
                Autostart = true,
                ConfigureChannel = channelOptions =>
                {
                    channelOptions.Endpoint = ev =>
                    {
                        var logger = server.GetRequiredService<ILogger<EventSourceDemo>>();
                        logger.LogInformation(ev.ToJson());
                    };
                }
            };

            var esInstance = await esManager.Create(options);
            await esManager.Start(esInstance);

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task EventSourceSeparateChannel()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("diag", (provider, channelOptions) =>
                    {
                        channelOptions.Endpoint = ev =>
                        {
                            var logger = provider.GetRequiredService<ILogger<EventSourceDemo>>();
                            logger.LogInformation(ev.ToJson());
                        };
                    })
                    .AddEventSource<AzureServiceBusCloudEventSource>();
            });

            var esManager = server.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                TargetChannelName = "diag",
                EventSourceDefinition = "AzureServiceBusCloudEventSource",
                Configuration = new AzureServiceBusCloudEventSourceConfiguration()
                {
                    QueueName = "sqlexecutor",
                    ConnectionString =
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4="
                },
                Autostart = true,
            };

            var esInstance = await esManager.Create(options);
            await esManager.Start(esInstance);

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task EventSourceDefaultChannel()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("diag", (provider, channelOptions) =>
                    {
                        channelOptions.Endpoint = ev =>
                        {
                            var logger = provider.GetRequiredService<ILogger<EventSourceDemo>>();
                            logger.LogInformation(ev.ToJson());
                        };
                    })
                    .AddEventSource<AzureServiceBusCloudEventSource>();

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "diag";
                });
            });

            var esManager = server.GetRequiredService<IEventSourceInstanceManager>();

            var options = new EventSourceInstanceOptions()
            {
                EventSourceDefinition = "AzureServiceBusCloudEventSource",
                Configuration = new AzureServiceBusCloudEventSourceConfiguration()
                {
                    QueueName = "sqlexecutor",
                    ConnectionString =
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4="
                },
                Autostart = true,
            };

            var esInstance = await esManager.Create(options);
            await esManager.Start(esInstance);

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task ServiceBusAndHttp()
        {
            var channel = CloudEventsChannelBuilder.From("diag")
                .Logger();

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(channel)
                    .AddHttpCloudEventSource("events")
                    .AddAzureServiceBusCloudEventSource(
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4=",
                        "myqueue");

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "diag";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task HandlingEvents()
        {
            var diagChannel = CloudEventsChannelBuilder.From("diag")
                .Logger()
                .Subscribe("bus");

            var bus = CloudEventsChannelBuilder.From("bus")
                .EventAggregator();

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(diagChannel)
                    .AddChannel(bus)
                    .AddHandler<MyHandler>()
                    .AddHttpCloudEventSource("events")
                    .AddAzureServiceBusCloudEventSource(
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4=",
                        "sqlexecutor");

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task ForwardingEvents()
        {
            var diagChannel = CloudEventsChannelBuilder.From("diag")
                .Logger()
                .Subscribe("bus");

            var bus = CloudEventsChannelBuilder.From("bus")
                .Encrypt(publicKeyPath: @"c:\temp\public.key")
                .Http("https://webhook.site/3e187a30-04d8-4383-8286-feec63ab0fdc");

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(diagChannel)
                    .AddChannel(bus)
                    .AddHttpCloudEventSource("events")
                    .AddAzureServiceBusCloudEventSource(
                        "Endpoint=sb://sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YDcvmuL4=",
                        "sqlexecutor");

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
        
        [Fact]
        public async Task Polling()
        {
            var bus = CloudEventsChannelBuilder.From("bus")
                .Http("https://webhook.site/3e187a30-04d8-4383-8286-feec63ab0fdc");

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(bus)
                    .AddEventSource<DemoPoller>(options =>
                    {
                        options.PollingFrequency = TimeSpan.FromSeconds(3);
                        options.Autostart = true;
                    });

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
        
        [Fact]
        public async Task StatefulPolling()
        {
            var bus = CloudEventsChannelBuilder.From("bus")
                .Http("https://webhook.site/3e187a30-04d8-4383-8286-feec63ab0fdc");

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(bus)
                    .AddEventSource<DemoPoller>(options =>
                    {
                        options.PollingFrequency = TimeSpan.FromSeconds(3);
                        options.Autostart = true;
                    });

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
        
        [Fact]
        public async Task Persistense()
        {
            var bus = CloudEventsChannelBuilder.From("bus")
                .Http("https://webhook.site/3e187a30-04d8-4383-8286-feec63ab0fdc");

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(bus)
                    .AddEventSource<DemoPoller>(options =>
                    {
                        options.PollingFrequency = TimeSpan.FromSeconds(3);
                        options.Autostart = true;
                        options.Id = "demo";
                    });

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Hosted()
        {
            var bus = CloudEventsChannelBuilder.From("bus")
                .Http("https://webhook.site/3e187a30-04d8-4383-8286-feec63ab0fdc");

            Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel(bus)
                    .AddEventSource<HostedPoller>(options =>
                    {
                        options.Autostart = true;
                    });

                services.Configure<DefaultChannelOptions>(options =>
                {
                    options.DefaultChannelName = "bus";
                });
            });

            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        public class HostedPoller : BackgroundService
        {
            private readonly ICloudEventPublisher _publisher;

            public HostedPoller(ICloudEventPublisher publisher)
            {
                _publisher = publisher;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                var count = 0;
                while (stoppingToken.IsCancellationRequested == false)
                {
                    await _publisher.Publish(new DemoEvent()
                    {
                        Count = count
                    });

                    count += 1;
                    
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                
            }
        }
        public class DemoEvent
        {
            public string Name { get; set; } = "Demo";
            public int Count { get; set; } = 0;
        }
        
        public class DemoPoller
        {
            public Task<(DemoEvent, int)> Poll(int currentCount)
            {
                var result = new DemoEvent();
                result.Count = currentCount;
                
                return Task.FromResult((result, currentCount + 1));
            }
        }

        public class MyHandler
        {
            private readonly ILogger<MyHandler> _logger;

            public MyHandler(ILogger<MyHandler> logger)
            {
                _logger = logger;
            }

            public Task Handle(CloudEvent ev)
            {
                _logger.LogError(ev.ToJson());

                return Task.CompletedTask;
            }
        }
    }
}

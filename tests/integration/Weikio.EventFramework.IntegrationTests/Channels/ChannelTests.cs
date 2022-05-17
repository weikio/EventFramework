using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.Extensions.EventAggregator;
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
            var server = Init(services =>
            {
                services.AddEventFramework();
            });

            var firstCounter = 0;
            var secondCounter = 0;

            var channel = await CloudEventsChannelBuilder.From()
                .WithName("testchannel")
                .Component(ev =>
                {
                    return ev;
                })
                .Component(context =>
                {
                    var comp = new CloudEventsComponent(ev =>
                    {
                        firstCounter += 1;

                        return ev;
                    });

                    return Task.FromResult(comp);
                })
                .Component(new FilterComponentBuilder(ev =>
                {
                    if (string.Equals(ev.Type, "CustomerCreatedEvent"))
                    {
                        return Filter.Continue;
                    }

                    return Filter.Skip;
                }))
                .Component(context =>
                {
                    var comp = new CloudEventsComponent(ev =>
                    {
                        secondCounter += 1;

                        return ev;
                    });

                    return Task.FromResult(comp);
                })
                .Build(server);

            server.GetRequiredService<IChannelManager>().Add(channel);

            var msg1 = new CustomerCreatedEvent();
            var msg2 = new CustomerDeletedEvent();
            var msg3 = new CustomerCreatedEvent();

            await channel.Send(msg1);
            await channel.Send(msg2);
            await channel.Send(msg3);

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(3, firstCounter);
            Assert.Equal(2, secondCounter);
        }

        [Fact]
        public async Task CanUseTypedComponentBuilders()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("other")
                    .AddChannel(CloudEventsChannelBuilder.From("local")
                        .Subscribe("other")
                        .EventAggregator())
                    .AddHandler<HandleCustomer>();
            });

            var publisher = server.GetRequiredService<ICloudEventPublisherFactory>().CreatePublisher("other");
            await publisher.Publish(new CustomerCreatedEvent() { Name = "MK" });

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        public class HandleCustomer
        {
            public Task Handle(CloudEvent ev)
            {
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task CanAutoAddInterceptorsToEachChannel()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("bus", (provider, options) =>
                    {
                    })
                    .AddChannel("bus2", (provider, options) =>
                    {
                    });

                services.AddTransient<ICloudEventPublisherBuilder, TestCloudEventPublisherBuilder>();

                services.Configure<DefaultCloudEventChannelOptions>(options =>
                {
                    options.InitAction = channel =>
                    {
                        var loggerInterceptor = new LoggerInterceptor();
                        channel.AddInterceptor((InterceptorTypeEnum.PreComponents, loggerInterceptor));
                    };
                });
            });

            var publisher = server.GetRequiredService<ICloudEventPublisher>();

            await publisher.Publish(new CustomerCreatedEvent(), "bus2");
            await publisher.Publish(new CustomerCreatedEvent(), "bus");
            await publisher.Publish(new CustomerCreatedEvent(), "bus2");

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(3,  LoggerInterceptor.Counter.Get());
        }

        [Fact]
        public async Task CanAutoAddInterceptorsToEachChannelUsingIConfigureOptions()
        {
            var server = Init(services =>
            {
                services.AddEventFramework()
                    .AddChannel("bus", (provider, options) =>
                    {
                    })
                    .AddChannel("bus2", (provider, options) =>
                    {
                    });

                services.AddTransient<ICloudEventPublisherBuilder, TestCloudEventPublisherBuilder>();

                services.AddTransient<IConfigureOptions<DefaultCloudEventChannelOptions>, InterceptorCreator>();
                services.AddSingleton<Counter>();
            });

            var publisher = server.GetRequiredService<ICloudEventPublisher>();

            await publisher.Publish(new CustomerCreatedEvent(), "bus2");
            await publisher.Publish(new CustomerCreatedEvent(), "bus");
            await publisher.Publish(new CustomerCreatedEvent(), "bus2");
            await publisher.Publish(new CustomerCreatedEvent(), "bus");
            await publisher.Publish(new CustomerCreatedEvent(), "bus");

            await Task.Delay(TimeSpan.FromSeconds(1));

            var counter = server.GetRequiredService<Counter>();
            
            Assert.Equal(5, counter.Get());
        }

        public class Counter
        {
            private int _count;

            public void Increment()
            {
                Interlocked.Increment(ref _count);
            }

            public int Get()
            {
                return _count;
            }
        }

        public class InterceptorCreator : IConfigureOptions<DefaultCloudEventChannelOptions>
        {
            private readonly IServiceProvider _serviceProvider;

            public InterceptorCreator(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public void Configure(DefaultCloudEventChannelOptions options)
            {
                options.InitAction = channel =>
                {
                    var counter = _serviceProvider.GetRequiredService<Counter>();
                    channel.AddInterceptor((InterceptorTypeEnum.PreComponents, new DiInterceptor(counter)));
                };
            }
        }

        [Fact]
        public async Task ChannelEndpointsAreNotDuplicated()
        {
            var count = 0;

            var server = Init(services =>
            {
                services.AddEventFramework()
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

    public class LoggerInterceptor : IChannelInterceptor
    {
        public static ChannelTests.Counter Counter = new ChannelTests.Counter();

        public Task<object> Intercept(object obj)
        {
            Counter.Increment();

            return Task.FromResult(obj);
        }
    }

    public class DiInterceptor : IChannelInterceptor
    {
        private readonly ChannelTests.Counter _counter;

        public DiInterceptor(ChannelTests.Counter counter)
        {
            _counter = counter;
        }

        public Task<object> Intercept(object obj)
        {
            _counter.Increment();

            return Task.FromResult(obj);
        }
    }
}

﻿using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.Extensions.EventAggregator;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventSource
{
    [Collection(nameof(NotThreadSafeResourceCollection))]
    public class EventSourceChannelTests : PollingEventSourceTestBase, IDisposable
    {
        public EventSourceChannelTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task EventSourceCanSendEventsToCustomChannel()
        {
            var serviceProvider = Init(services =>
            {
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddEventSource<StatelessTestEventSource>();
            
            });

            var received = false;

            var channel = new CloudEventsChannel("customChannel", ev =>
            {
                received = true;
            });
            
            var channelManager = serviceProvider.GetRequiredService<IChannelManager>();
            channelManager.Add(channel);

            var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            
            var options = new EventSourceInstanceOptions()
            {
                PollingFrequency = TimeSpan.FromSeconds(1), 
                EventSourceDefinition = "StatelessTestEventSource", 
                TargetChannelName = "customChannel", 
                Autostart = true
            };
            
            await eventSourceInstanceManager.Create(options);

            await ContinueWhen(() => received);
        }
        
        [Fact]
        public Task EventSourceEventsCanBeHandled()
        {
            throw new NotImplementedException();

            // var handledMessages = 0;
            // var serviceProvider = Init(services =>
            // {
            //     services.AddCloudEventSources();
            //     services.AddCloudEventPublisher();
            //     services.AddEventSource<StatelessTestEventSource>();
            //
            //     services.AddHandler(ev =>
            //         {
            //             handledMessages += 1;
            //         }
            //     );
            // });

            // var channel = new CloudEventsDataflowChannel("customChannel", ev =>
            // {
            //
            // });
            //
            // var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            // var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
            //
            // var localChannel = ActivatorUtilities.CreateInstance<NewLocalChannel>(serviceProvider);
            // channelManager.Add("local", localChannel);
            //
            // var options = new EventSourceInstanceOptions()
            // {
            //     PollingFrequency = TimeSpan.FromSeconds(1), 
            //     EventSourceDefinition = "StatelessTestEventSource", 
            //     TargetChannelName = "local", 
            //     Autostart = true
            // };
        }

        // [Fact]
        // public async Task EventSourceUsesPrivateChannel()
        // {
        //     var serviceProvider = Init(services =>
        //     {
        //         services.AddCloudEventSources();
        //         services.AddCloudEventPublisher();
        //         services.AddLocal();
        //         services.AddEventSource<StatelessTestEventSource>();
        //     });
        //
        //     var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
        //     var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
        //
        //     var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource" };
        //
        //     await eventSourceInstanceManager.Create(options);
        //
        //     await ContinueWhen(() => channelManager.Channels.Any());
        // }

        // [Fact]
        // public async Task AutomaticallyCreatedEventSourceChannelTargetsDefaultChannel()
        // {
        //     var serviceProvider = Init(services =>
        //     {
        //         services.AddCloudEventSources();
        //         services.AddCloudEventPublisher();
        //         services.AddLocal();
        //         services.AddEventSource<StatelessTestEventSource>();
        //     });
        //
        //     var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
        //     var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
        //
        //     var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource" };
        //
        //     await eventSourceInstanceManager.Create(options);
        //
        //     await ContinueWhen(() => channelManager.Channels.Any());
        // }
        //
        // [Fact]
        // public async Task CanHandleDataflowChannelEvents()
        // {
        //     var handledMessages = 0;
        //
        //     var serviceProvider = Init(services =>
        //     {
        //         services.AddCloudEventSources();
        //         services.AddCloudEventPublisher();
        //         services.AddEventSource<StatelessTestEventSource>();
        //
        //         services.AddHandler(ev =>
        //         {
        //             handledMessages += 1;
        //         });
        //     });
        //
        //     var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
        //     var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
        //
        //     var localChannel = ActivatorUtilities.CreateInstance<NewLocalChannel>(serviceProvider);
        //     channelManager.Add("local", localChannel);
        //
        //     var options = new EventSourceInstanceOptions()
        //     {
        //         PollingFrequency = TimeSpan.FromSeconds(1), 
        //         EventSourceDefinition = "StatelessTestEventSource", 
        //         TargetChannelName = "local", 
        //         Autostart = true
        //     };
        //
        //     await eventSourceInstanceManager.Create(options);
        //
        //     await ContinueWhen(() => handledMessages > 0);
        // }
    }
}

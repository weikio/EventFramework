// using System;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Threading.Tasks;
// using EventFrameworkTestBed;
// using EventFrameworkTestBed.Events;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Extensions.DependencyInjection;
// using Weikio.ApiFramework.Core.Endpoints;
// using Weikio.EventFramework.Abstractions;
// using Weikio.EventFramework.AspNetCore.Extensions;
// using Weikio.EventFramework.EventCreator;
// using Weikio.EventFramework.EventGateway;
// using Weikio.EventFramework.EventGateway.Gateways.Local;
// using Weikio.EventFramework.EventPublisher;
// using Weikio.EventFramework.EventSource;
// using Weikio.EventFramework.EventSource.Abstractions;
// using Weikio.EventFramework.EventSource.EventSourceWrapping;
// using Weikio.EventFramework.Extensions.EventAggregator;
// using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
// using Weikio.EventFramework.IntegrationTests.Infrastructure;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace Weikio.EventFramework.IntegrationTests.EventSource
// {
//     [Collection(nameof(NotThreadSafeResourceCollection))]
//     public class EventSourceChannelTests : PollingEventSourceTestBase, IDisposable
//     {
//         public EventSourceChannelTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
//         {
//         }
//
//         [Fact]
//         public async Task EventSourceUsesPrivateChannel()
//         {
//             var serviceProvider = Init(services =>
//             {
//                 services.AddCloudEventSources();
//                 services.AddCloudEventPublisher();
//                 services.AddLocal();
//                 services.AddEventSource<StatelessTestEventSource>();
//             });
//
//             var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
//             var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
//
//             var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource" };
//
//             await eventSourceInstanceManager.Create(options);
//
//             await ContinueWhen(() => channelManager.Channels.Any());
//         }
//
//         [Fact]
//         public async Task AutomaticallyCreatedEventSourceChannelTargetsDefaultChannel()
//         {
//             var serviceProvider = Init(services =>
//             {
//                 services.AddCloudEventSources();
//                 services.AddCloudEventPublisher();
//                 services.AddLocal();
//                 services.AddEventSource<StatelessTestEventSource>();
//             });
//
//             var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
//             var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
//
//             var options = new EventSourceInstanceOptions() { PollingFrequency = TimeSpan.FromSeconds(1), EventSourceDefinition = "StatelessTestEventSource" };
//
//             await eventSourceInstanceManager.Create(options);
//
//             await ContinueWhen(() => channelManager.Channels.Any());
//         }
//
//         [Fact]
//         public async Task CanHandleDataflowChannelEvents()
//         {
//             var handledMessages = 0;
//
//             var serviceProvider = Init(services =>
//             {
//                 services.AddCloudEventSources();
//                 services.AddCloudEventPublisher();
//                 services.AddEventSource<StatelessTestEventSource>();
//
//                 services.AddHandler(ev =>
//                 {
//                     handledMessages += 1;
//                 });
//             });
//
//             var eventSourceInstanceManager = serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
//             var channelManager = serviceProvider.GetRequiredService<ICloudEventChannelManager>();
//
//             var localChannel = ActivatorUtilities.CreateInstance<NewLocalChannel>(serviceProvider);
//             channelManager.Add("local", localChannel);
//
//             var options = new EventSourceInstanceOptions()
//             {
//                 PollingFrequency = TimeSpan.FromSeconds(1), 
//                 EventSourceDefinition = "StatelessTestEventSource", 
//                 TargetChannelName = "local", 
//                 Autostart = true
//             };
//
//             await eventSourceInstanceManager.Create(options);
//
//             await ContinueWhen(() => handledMessages > 0);
//         }
//     }
// }

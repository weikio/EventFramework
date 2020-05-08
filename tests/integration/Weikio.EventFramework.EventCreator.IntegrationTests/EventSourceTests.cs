using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.EventCreator.IntegrationTests
{
    public class EventSourceTests : EventFrameworkTestBase, IDisposable
    {
        private TestCloudEventPublisher _testCloudEventPublisher;
        private int _counter;

        public EventSourceTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
            _testCloudEventPublisher = new TestCloudEventPublisher();
            _counter = 0;
        }

        [Fact]
        public async Task CanAddHostedServiceAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddContinuousTypeAsEventSource()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task CanAddLongPollingTypeAsEventSource()
        {
            // yield return 
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddHostedService(provider =>
                {
                    var publisher = provider.GetRequiredService<ICloudEventPublisher>();
                    var s = new ContinuousTestEventSource();
                    
                    var source = new ContinuousEventSourceHost(s.CheckForNewFiles, publisher);

                    return source;
                });
            });

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }

        
        

        [Fact]
        public async Task CanAddEventReturningTypeAsEventSource()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<TestEventSource>(pollingFrequency: TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task CanAddEventReturningTypeWithMultipleMethodsAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddPublishingTypeAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddPublishingTypeWithStateAsEventSource()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanAddStatelessEventSource()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource(() =>
                {
                    return new CustomerCreatedEvent();
                }, TimeSpan.FromSeconds(1));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.NotEmpty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task StatelessEventSourceIsNotRunInStart()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource(() => new CustomerCreatedEvent(), TimeSpan.FromMinutes(3));
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task StatefullEventSourceIsInitializedInStart()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<int>(currentCount =>
                {
                    _counter = currentCount + 10;

                    return (new CustomerCreatedEvent(), _counter);
                }, TimeSpan.FromMinutes(5));
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Equal(10, _counter);

            // Events aren't published yet as this is just an initialization run
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        [Fact]
        public async Task StatefullEventSourceWithCronIntervalIsInitializedInStart()
        {
            var server = Init(services =>
            {
                services.AddSingleton(typeof(ICloudEventPublisher), _testCloudEventPublisher);
                services.AddCloudEventSources();
                services.AddCloudEventPublisher();
                services.AddLocal();

                services.AddSource<int>(currentCount =>
                {
                    _counter = currentCount + 10;

                    return (new CustomerCreatedEvent(), _counter);
                }, cronExpression: "0 0 12 * * ?");
            });

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Equal(10, _counter);

            // Events aren't published yet as this is just an initialization run
            Assert.Empty(_testCloudEventPublisher.PublishedEvents);
        }

        public void Dispose()
        {
            
        }
    }

    public class CounterUpdatedEvent
    {
        public int Count { get; }

        public CounterUpdatedEvent(int count)
        {
            Count = count;
        }
    }
    
    

    public class ContinuousTestEventSource
    {
        public async IAsyncEnumerable<NewFileEvent> CheckForNewFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var originalFiles = new List<string>() { "file1.txt", "file2.txt" };

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                var currentFiles = new List<string>(originalFiles) { Guid.NewGuid().ToString() + ".txt" };

                var result = currentFiles.Except(originalFiles).ToList();

                if (result.Any() == false)
                {
                    continue;
                }

                foreach (var res in result)
                {
                    var newFileEvent = new NewFileEvent(res);

                    yield return newFileEvent;
                }

                originalFiles = currentFiles;
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    public class TestEventSource
    {
        public Task<(List<NewFileEvent> NewEvents, List<string> NewState)> CheckForNewFiles(List<string> currentState)
        {
            List<string> files;

            if (currentState == null)
            {
                files = new List<string>() { "file1.txt", "file2.txt" };
            }
            else
            {
                files = new List<string>() { "file1.txt", "file2.txt", "file3.txt" };
            }

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return Task.FromResult<(List<NewFileEvent> NewEvents, List<string> NewState)>((null, files));
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            return Task.FromResult((newEvents, files));
        }
    }

    public class TestEventSourceWithMultipleEventTypes
    {
        public async Task<(List<NewFileEvent> NewEvents, List<string> NewState)> CheckForNewFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return (default, files);
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            return (newEvents, files);
        }

        public async Task<(List<DeletedFileEvent> NewEvents, List<string> NewState)> CheckForDeletedFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var newEvents = new List<DeletedFileEvent>();

            if (currentState?.Any() == true)
            {
                var result = files.Except(currentState).ToList();

                foreach (var file in currentState)
                {
                    if (result.Contains(file))
                    {
                        continue;
                    }

                    newEvents.Add(new DeletedFileEvent(file));
                }
            }

            if (newEvents.Any())
            {
                return (newEvents, files);
            }

            return (default, files);
        }
    }

    public class PublishingTestEventSource
    {
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public PublishingTestEventSource(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        public async Task<List<string>> CheckForNewFiles(List<string> currentState)
        {
            var files = new List<string>() { "file1.txt", "file2.txt" };

            var result = new List<string>(files);

            if (currentState?.Any() == true)
            {
                result = files.Except(currentState).ToList();
            }

            if (!result.Any())
            {
                return files;
            }

            var newEvents = new List<NewFileEvent>();

            foreach (var res in result)
            {
                newEvents.Add(new NewFileEvent(res));
            }

            await _cloudEventPublisher.Publish(newEvents);

            return files;
        }
    }

    public class NewFileEvent
    {
        public string FileName { get; }

        public NewFileEvent(string fileName)
        {
            FileName = fileName;
        }
    }

    public class DeletedFileEvent
    {
        public string FileName { get; }

        public DeletedFileEvent(string fileName)
        {
            FileName = fileName;
        }
    }

    public class TestCloudEventPublisher : ICloudEventPublisher
    {
        public List<object> PublishedEvents = new List<object>();

        public TestCloudEventPublisher()
        {
        }

        public Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(cloudEvent);

            return Task.FromResult<CloudEvent>(cloudEvent);
        }

        public Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            PublishedEvents.AddRange(objects);

            return Task.FromResult(new List<CloudEvent>());
        }

        public Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(obj);

            return Task.FromResult(new CloudEvent("test", new Uri("http://localhost", UriKind.Absolute), Guid.NewGuid().ToString()));
        }
    }
}

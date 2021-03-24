using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.Polling;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventSource
{
    public abstract class PollingEventSourceTestBase : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private IServiceProvider _serviceProvider;
        
        protected HttpClient Client { get; set; }
        
        public ITestOutputHelper Output { get; set; }

        protected PollingEventSourceTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                    
                    services.AddCloudEventCreator();
                    services.AddTransient<ICloudEventPublisherBuilder, TestCloudEventPublisherBuilder>();
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    XUnitLoggerExtensions.AddXUnit((ILoggingBuilder) logging, Output); // Use the ITestOutputHelper instance
                });
                
            });

            MyTestCloudEventPublisher.PublishedEvents = new List<CloudEvent>();

            _serviceProvider = server.Services;
            Client = server.CreateClient();
            
            return _serviceProvider;
        }
        
        
        public async Task ContinueWhen(Predicate<List<CloudEvent>> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe(MyTestCloudEventPublisher.PublishedEvents);

                if (success)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
            }

            if (success)
            {
                return;
            }

            throw new Exception(assertErrorMessage ?? "Assertion failed");
        }
        
        public async Task ContinueWhen(Func<bool> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe();

                if (success)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
            }

            if (success)
            {
                return;
            }

            throw new Exception(assertErrorMessage ?? "Assertion failed");
        }

        public void Dispose()
        {
            var eventSourceInstanceManager = _serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            eventSourceInstanceManager.StopAll();
            
            _factory.Dispose();
            MyTestCloudEventPublisher.PublishedEvents = new List<CloudEvent>();
        }
    }
    
        public abstract class ChannelTestBase : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private IServiceProvider _serviceProvider;
        
        protected HttpClient Client { get; set; }
        
        public ITestOutputHelper Output { get; set; }

        protected ChannelTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                    services.AddTransient<ICloudEventPublisherBuilder, TestCloudEventPublisherBuilder>();
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    XUnitLoggerExtensions.AddXUnit((ILoggingBuilder) logging, Output); // Use the ITestOutputHelper instance
                });
                
            });

            MyTestCloudEventPublisher.PublishedEvents = new List<CloudEvent>();

            _serviceProvider = server.Services;
            Client = server.CreateClient();
            
            return _serviceProvider;
        }
        
        
        public async Task ContinueWhen(Predicate<List<CloudEvent>> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe(MyTestCloudEventPublisher.PublishedEvents);

                if (success)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
            }

            if (success)
            {
                return;
            }

            throw new Exception(assertErrorMessage ?? "Assertion failed");
        }
        
        public async Task ContinueWhen(Func<bool> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe();

                if (success)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
            }

            if (success)
            {
                return;
            }

            throw new Exception(assertErrorMessage ?? "Assertion failed");
        }

        public void Dispose()
        {
            var eventSourceInstanceManager = _serviceProvider.GetRequiredService<IEventSourceInstanceManager>();
            eventSourceInstanceManager.StopAll();
            
            _factory.Dispose();
            MyTestCloudEventPublisher.PublishedEvents = new List<CloudEvent>();
        }
    }

}

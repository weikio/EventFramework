using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.ApiFramework
{
    public abstract class EventFrameworkTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        // Must be set in each test
        public ITestOutputHelper Output { get; set; }
        
        protected EventFrameworkTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected void Init(Action<IServiceCollection> action = null)
        {
            var webBuilder = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    action?.Invoke(services);
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    logging.AddXUnit(Output); // Use the ITestOutputHelper instance
                });

            });

            Client = webBuilder.CreateClient();
            Provider = webBuilder.Services;
        }

        public IServiceProvider Provider { get; set; }

        public HttpClient Client { get; set; }
        
        public async Task ContinueWhen(Func<Task<bool>> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
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
                try
                {
                    success = await probe();
                }
                catch (Exception)
                {
                    // ignored
                }

                if (success)
                {
                    break;
                }

                if (cts.Token.IsCancellationRequested)
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
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.AspNetCore.Extensions;
using Weikio.EventFramework.EventGateway;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public abstract class IntegrationFlowTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        // Must be set in each test
        public ITestOutputHelper Output { get; set; }
        
        protected IntegrationFlowTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var result = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddEventFramework();
                    action?.Invoke(services);
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    XUnitLoggerExtensions.AddXUnit((ILoggingBuilder) logging, Output); // Use the ITestOutputHelper instance
                });

            });

            return result.Services;
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
    }
}

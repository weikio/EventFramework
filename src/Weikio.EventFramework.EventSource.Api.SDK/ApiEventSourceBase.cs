using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public abstract class ApiEventSourceBase : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly IApiEventSourceConfiguration _configuration;
        protected abstract Type ApiEventSourceType { get; }
        protected abstract Type ApiEventSourceConfigurationType { get; }

        public ApiEventSourceBase(IServiceProvider serviceProvider, ICloudEventPublisher cloudEventPublisher, IApiEventSourceConfiguration configuration = null)
        {
            _serviceProvider = serviceProvider;
            _cloudEventPublisher = cloudEventPublisher;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<ApiEventSourceBase>>();
            logger.LogInformation("Initializing API event source with configuration {Configuration}", _configuration);
            
            var factory = _serviceProvider.GetRequiredService<ApiEventSourceFactory>();
            await factory.Create(ApiEventSourceConfigurationType, ApiEventSourceType, _cloudEventPublisher, _configuration, stoppingToken);
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    public class ApiEventSourceRunner : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceDefinitionConfigurationTypeProvider _configurationTypeProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly IApiEventSourceConfiguration _configuration;
        protected Type ApiEventSourceType { get; set; }
        protected Type ApiEventSourceConfigurationType { get; set; }

        public ApiEventSourceRunner(IServiceProvider serviceProvider, IEventSourceDefinitionConfigurationTypeProvider configurationTypeProvider, ICloudEventPublisher cloudEventPublisher, IApiEventSourceConfiguration configuration = null)
        {
            _serviceProvider = serviceProvider;
            _configurationTypeProvider = configurationTypeProvider;
            _cloudEventPublisher = cloudEventPublisher;
            _configuration = configuration;
        }

        public void Initialize(Type apiEventSourceType)
        {
            ApiEventSourceType = apiEventSourceType;
            ApiEventSourceConfigurationType = _configurationTypeProvider.Get(apiEventSourceType);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<ApiEventSourceRunner>>();
            logger.LogInformation("Initializing API event source with configuration {Configuration}", _configuration);
            
            var factory = _serviceProvider.GetRequiredService<ApiEventSourceFactory>();
            await factory.Create(ApiEventSourceConfigurationType, ApiEventSourceType, _cloudEventPublisher, _configuration, stoppingToken);
        }
    }
}

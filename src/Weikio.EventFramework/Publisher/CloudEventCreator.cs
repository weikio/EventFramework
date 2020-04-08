using System;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Configuration;

namespace Weikio.EventFramework.Publisher
{
    public class CloudEventCreator : ICloudEventCreator
    {
        private readonly ILogger<CloudEventCreator> _logger;
        private readonly EventFrameworkOptions _options;
        private readonly IOptionsMonitor<CloudEventPublisherOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;

        public CloudEventCreator(ILogger<CloudEventCreator> logger, IOptions<EventFrameworkOptions> options, IOptionsMonitor<CloudEventPublisherOptions> optionsMonitor, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options.Value;
            _optionsMonitor = optionsMonitor;
            _serviceProvider = serviceProvider;
        }

        public CloudEvent CreateCloudEvent(object obj, string eventTypeName, string id, Uri source, ICloudEventExtension[] extensions = null,
            string subject = null)
        {
            var options = _optionsMonitor.Get(obj.GetType().FullName);

            if (string.IsNullOrEmpty(eventTypeName))
            {
                eventTypeName = options.GetEventTypeName(options, _serviceProvider, obj);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = options.GetId(options, _serviceProvider, obj);
            }

            if (extensions == null)
            {
                extensions = options.GetExtensions(options, _serviceProvider, obj);
            }

            if (subject == null)
            {
                subject = options.GetSubject(options, _serviceProvider, obj);
            }

            if (source == null)
            {
                source = _options.DefaultSource;
            }

            try
            {
                var cloudEvent = new CloudEvent(eventTypeName, source, id, DateTime.UtcNow, extensions) { Data = obj, Subject = subject };

                return cloudEvent;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create cloud event from {Object}", obj);
                throw;
            }
        }
    }
}

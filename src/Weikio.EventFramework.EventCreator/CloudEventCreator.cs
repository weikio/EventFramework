using System;
using System.Collections.Generic;
using System.Linq;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventCreator
{
    public class CloudEventCreator : ICloudEventCreator
    {
        private readonly ILogger<CloudEventCreator> _logger;
        private readonly IOptionsMonitor<EventCreationOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;

        public CloudEventCreator(ILogger<CloudEventCreator> logger, IOptionsMonitor<EventCreationOptions> optionsMonitor, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _serviceProvider = serviceProvider;
        }

        public CloudEvent CreateCloudEvent(object obj, string eventTypeName = null, string id = null, Uri source = null,
            ICloudEventExtension[] extensions = null,
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
                source = options.GetSource(options, _serviceProvider, obj);
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

        public IEnumerable<CloudEvent> CreateCloudEvents(IEnumerable<object> objects, string eventTypeName = null, string id = null, Uri source = null,
            ICloudEventExtension[] extensions = null, string subject = null)
        {
            if (objects == null)
            {
                return new List<CloudEvent>();
            }

            var result = new List<CloudEvent>();
            var index = 0;

            foreach (var obj in objects)
            {
                var sequenceExtension = new IntegerSequenceExtension(index);

                var originalExtensions = extensions;
                
                if (extensions?.Any() != true)
                {
                    extensions = new ICloudEventExtension[] { sequenceExtension };
                }
                else
                {
                    var updatedExtensions = new List<ICloudEventExtension>(extensions) { sequenceExtension };

                    extensions = updatedExtensions.ToArray();
                }

                var cloudEvent = CreateCloudEvent(obj, eventTypeName, id, source, extensions, subject);
                result.Add(cloudEvent);

                extensions = originalExtensions;
                index += 1;
            }

            return result;
        }
    }
}

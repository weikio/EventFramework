using System;
using System.Collections.Generic;
using System.Linq;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventCreator
{
    public class CloudEventCreator : ICloudEventCreator
    {
        private readonly ILogger<CloudEventCreator> _logger;
        private readonly ICloudEventCreatorOptionsProvider _cloudEventCreatorOptionsProvider;
        private readonly IServiceProvider _serviceProvider;

        public CloudEventCreator(CloudEventCreationOptions options = null)
        {
            _logger = new NullLogger<CloudEventCreator>();
            _cloudEventCreatorOptionsProvider = new CloudEventCreatorOptions(options);
            _serviceProvider = null;
        }

        public CloudEventCreator(ILogger<CloudEventCreator> logger, ICloudEventCreatorOptionsProvider cloudEventCreatorOptionsProvider,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _cloudEventCreatorOptionsProvider = cloudEventCreatorOptionsProvider;
            _serviceProvider = serviceProvider;
        }

        public CloudEvent CreateCloudEvent(object obj, string eventTypeName = null, string id = null, Uri source = null,
            ICloudEventExtension[] extensions = null,
            string subject = null)
        {
            var options = _cloudEventCreatorOptionsProvider.Get(obj.GetType().FullName);

            try
            {
                var result = Create(obj, options, eventTypeName, id, source, extensions, subject, _serviceProvider);

                return result;
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
                var originalExtensions = GetSequenceExtension(ref extensions, index);

                var cloudEvent = CreateCloudEvent(obj, eventTypeName, id, source, extensions, subject);
                result.Add(cloudEvent);

                extensions = originalExtensions;
                index += 1;
            }

            return result;
        }

        public static CloudEvent Create(object obj, CloudEventCreationOptions options = null, string eventTypeName = null, string id = null, Uri source = null,
            ICloudEventExtension[] extensions = null,
            string subject = null, IServiceProvider serviceProvider = null)
        {
            if (options == null)
            {
                options = new CloudEventCreationOptions();
            }

            if (string.IsNullOrEmpty(eventTypeName))
            {
                eventTypeName = options.GetEventTypeName(options, serviceProvider, obj);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = options.GetId(options, serviceProvider, obj);
            }

            if (extensions == null)
            {
                extensions = options.GetExtensions(options, serviceProvider, obj);
            }

            if (subject == null)
            {
                subject = options.GetSubject(options, serviceProvider, obj);
            }

            if (source == null)
            {
                source = options.GetSource(options, serviceProvider, obj);
            }

            try
            {
                var cloudEvent = new CloudEvent(eventTypeName, source, id, DateTime.UtcNow, extensions) { Data = obj, Subject = subject };

                return cloudEvent;
            }
            catch (Exception e)
            {
                throw new FailedToCreateCloudEventException(e);
            }
        }

        public static string CreateJson(object obj, CloudEventCreationOptions options = null, string eventTypeName = null, string id = null, Uri source = null,
            ICloudEventExtension[] extensions = null,
            string subject = null, IServiceProvider serviceProvider = null)
        {
            var cloudEvent = Create(obj, options, eventTypeName, id, source, extensions, subject, serviceProvider);
            var result = cloudEvent.ToJson();

            return result;
        }

        public static IEnumerable<CloudEvent> Create(IEnumerable<object> objects, CloudEventCreationOptions options = null, string eventTypeName = null,
            string id = null, Uri source = null,
            ICloudEventExtension[] extensions = null, string subject = null, IServiceProvider serviceProvider = null)
        {
            if (objects == null)
            {
                return new List<CloudEvent>();
            }

            var result = new List<CloudEvent>();
            var index = 0;

            foreach (var obj in objects)
            {
                var originalExtensions = GetSequenceExtension(ref extensions, index);

                var cloudEvent = Create(obj, options, eventTypeName, id, source, extensions, subject, serviceProvider);
                result.Add(cloudEvent);

                extensions = originalExtensions;
                index += 1;
            }

            return result;
        }

        public static string CreateJson(IEnumerable<object> objects, CloudEventCreationOptions options = null, string eventTypeName = null, string id = null,
            Uri source = null,
            ICloudEventExtension[] extensions = null, string subject = null, IServiceProvider serviceProvider = null)
        {
            var cloudEvents = Create(objects, options, eventTypeName, id, source, extensions, subject, serviceProvider);

            var result = new JArray();

            foreach (var cloudEvent in cloudEvents)
            {
                var jObject = cloudEvent.ToJObject();
                result.Add(jObject);
            }

            return result.ToString(Formatting.Indented);
        }

        private static ICloudEventExtension[] GetSequenceExtension(ref ICloudEventExtension[] extensions, int index)
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

            return originalExtensions;
        }
    }
}

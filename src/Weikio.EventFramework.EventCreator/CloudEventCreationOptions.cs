using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventCreator
{
    public interface ICloudEventDefinitionManager
    {
        void Add(CloudEventDefinition definition);
        List<CloudEventDefinition> List();
        Task Register(CloudEvent ev);
    }

    public class DefaultCloudEventDefinitionManager : List<CloudEventDefinition>, ICloudEventDefinitionManager
    {
        private readonly ILogger<DefaultCloudEventDefinitionManager> _logger;

        public DefaultCloudEventDefinitionManager(IEnumerable<CloudEventDefinition> initialDefinitions, ILogger<DefaultCloudEventDefinitionManager> logger)
        {
            _logger = logger;
            AddRange(initialDefinitions);
        }

        public List<CloudEventDefinition> List()
        {
            return this;
        }

        public async Task Register(CloudEvent ev)
        {
            var result = new CloudEventDefinition { Source = ev.Source.ToString(), Type = ev.Type };

            JsonSchema schema;
            if (ev.DataSchema != null)
            {
                try
                {
                    schema = await JsonSchema.FromUrlAsync(ev.DataSchema.ToString());
                    result.DataSchema = JsonConvert.SerializeObject(schema, Formatting.Indented);

                    Add(result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to register cloud event based on schema {SchemaUri}", ev.DataSchema);
                }
            }
            
            if (ev.Data == null)
            {
                Add(result);

                return;
            }

            if (ev.Data is JToken token)
            {
                var json = token.ToString();
                schema = JsonSchema.FromSampleJson(json);
            }
            else if (ev.Data is string str)
            {
                schema = JsonSchema.FromSampleJson(str);
            }
            else
            {
                schema = JsonSchema.FromType(ev.Data.GetType());
            }

            result.DataSchema = JsonConvert.SerializeObject(schema, Formatting.Indented);
            
            Add(result);
        }
    }

    public class CloudEventCreationOptions
    {
        public string EventTypeName { get; set; }
        public CloudEventsSpecVersion SpecVersion { get; set; } = CloudEventsSpecVersion.V1_0;
        public Uri Source { get; set; } = new Uri("http://localhost/eventframework");

        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetEventTypeName { get; set; } = (options, provider, o) =>
        {
            if (!string.IsNullOrWhiteSpace(options.EventTypeName))
            {
                return options.EventTypeName;
            }

            return o.GetType().Name;
        };

        public string Subject { get; set; } = string.Empty;
        public string DataContentType { get; set; } = "Application/Json";

        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetDataContentType { get; set; } =
            (options, provider, o) => options.DataContentType;

        public ICloudEventExtension[] AdditionalExtensions = Array.Empty<ICloudEventExtension>();

        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetSubject { get; set; } = (options, provider, o) => options.Subject;
        public Func<CloudEventCreationOptions, IServiceProvider, object, string> GetId { get; set; } = (options, provider, o) => Guid.NewGuid().ToString();

        public Func<CloudEventCreationOptions, IServiceProvider, object, Uri> GetSource { get; set; } = (options, provider, o) =>
        {
            if (options?.Source != null)
            {
                return options.Source;
            }

            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<CloudEventCreationOptions>>();
            var defaultOptions = optionsMonitor.CurrentValue;

            return defaultOptions.Source;
        };

        public Func<CloudEventCreationOptions, IServiceProvider, object, ICloudEventExtension[]> GetExtensions { get; set; } =
            (options, provider, o) => options.AdditionalExtensions;
    }
}

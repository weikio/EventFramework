using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventDefinition
{
    public class CloudEventToDefinitionConverter
    {
        private readonly ILogger<CloudEventToDefinitionConverter> _logger;

        public CloudEventToDefinitionConverter(ILogger<CloudEventToDefinitionConverter> logger)
        {
            _logger = logger;
        }

        public async Task<CloudEventDefinition> Convert(CloudEvent ev)
        {
            var result = new CloudEventDefinition(ev.Source.ToString(), ev.Type, contentType: ev.DataContentType);

            JsonSchema schema;
            if (ev.DataSchema != null)
            {
                try
                {
                    schema = await JsonSchema.FromUrlAsync(ev.DataSchema.ToString());
                    result.DataSchema = JsonConvert.SerializeObject(schema, Formatting.Indented);
                    result.DataSchemaUri = ev.DataSchema;

                    return result;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to register cloud event based on schema {SchemaUri}. Falling back to parsing the schema from the event", ev.DataSchema);
                }
            }
            
            if (ev.Data == null)
            {
                return result;
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

            return result;
        }
    }
}

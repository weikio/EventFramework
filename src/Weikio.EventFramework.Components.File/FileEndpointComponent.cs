using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.File
{
    public class FileEndpointComponent : CloudEventsComponent
    {
        private readonly FileEndpointOptions _configuration;
        private readonly ILogger<FileEndpointComponent> _logger;

        public FileEndpointComponent(FileEndpointOptions configuration, ILogger<FileEndpointComponent> logger)
        {
            _configuration = configuration;
            _logger = logger;

            Func = WriteFile;
        }

        public async Task<CloudEvent> WriteFile(CloudEvent cloudEvent)
        {
            var folder = _configuration.GetFolder(_configuration, cloudEvent);
            var filename = _configuration.GetFileName(_configuration, cloudEvent);
            var fullPath = Path.Combine(folder, filename);
            
            try
            {
                Directory.CreateDirectory(folder);

                await System.IO.File.WriteAllTextAsync(fullPath, cloudEvent.ToJson(), Encoding.UTF8);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to write cloud event {CloudEvent} to file {FilePath}", cloudEvent, fullPath);
            }

            return cloudEvent;
        }
    }
}

using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Samples.CodeConfiguration
{
    public class LoadHandler
    {
        public Task LoadFile(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class SaveHandler
    {
        private readonly ILogger<SaveHandler> _logger;

        public string Path { get; set; }
        
        public SaveHandler(ILogger<SaveHandler> logger)
        {
            _logger = logger;
        }

        public Task Test(CloudEvent cloudEvent, string eventType = "new-file")
        {
            _logger.LogInformation("Saving {CloudEvent} to {Path}", cloudEvent, Path);
            return Task.CompletedTask;
        }

        public Task NewFile(CloudEvent cloudEvent)
        {
            _logger.LogInformation("Creating new file with {CloudEvent} to {Path}", cloudEvent, Path);
            return Task.CompletedTask;
        }
        
        public Task MyGeneric(CloudEvent<CustomerCreated> cloudEvent)
        {
            return Task.CompletedTask;
        }

        public Task MyTyped(CustomerCreated cloudEvent)
        {
            return Task.CompletedTask;
        }

        // public Task SaveFile(CloudEvent cloudEvent, string eventType = "new-file")
        // {
        //     return Task.CompletedTask;
        // }
        //
        // public Task DeleteFile(CloudEvent cloudEvent, string eventType = "delete-file")
        // {
        //     return Task.CompletedTask;
        // }
    }
}

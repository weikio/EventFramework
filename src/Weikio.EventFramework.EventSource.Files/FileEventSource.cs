using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Files
{
    [DisplayName("FileEventSource")]
    public class FileEventSource : IHostedService
    {
        private readonly ILogger<FileEventSource> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly FileEventSourceConfiguration _configuration;
        private FileSystemWatcher _fileSystemWatcher;

        public FileEventSource(ILogger<FileEventSource> logger, ICloudEventPublisher cloudEventPublisher, FileEventSourceConfiguration configuration = null)
        {
            _logger = logger;
            _cloudEventPublisher = cloudEventPublisher;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            if (_configuration == null)
            {
                throw new Exception("Configuration is required");
            }

            _fileSystemWatcher = new FileSystemWatcher(_configuration.Folder, _configuration.Filter) { IncludeSubdirectories = _configuration.IncludeSubfolders };

            _fileSystemWatcher.Created += (sender, args) =>
            {
                _cloudEventPublisher.Publish(new FileCreatedEvent(args.Name, args.FullPath));
            };

            _fileSystemWatcher.Deleted += (sender, args) =>
            {
                _cloudEventPublisher.Publish(new FileDeletedEvent(args.Name, args.FullPath));
            };

            _fileSystemWatcher.Changed += (sender, args) =>
            {
            };

            _fileSystemWatcher.Renamed += (sender, args) =>
            {
                _cloudEventPublisher.Publish(new FileRenamed(args.Name, args.FullPath, args.OldName, args.OldFullPath));
            };

            _fileSystemWatcher.EnableRaisingEvents = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }
}

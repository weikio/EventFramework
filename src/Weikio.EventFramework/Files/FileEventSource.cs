using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Files
{
    public class EventSourceHostingService : IHostedService, IDisposable
    {
        private readonly ILogger<EventSourceHostingService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Func<Task> _start;
        private Func<Task> _stop;
        private Func<List<object>> _run;
        private Action _dispose;

        public EventSourceHostingService(ILogger<EventSourceHostingService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Func<Task> start = null, Func<Task> stop = null, Func<List<object>> run = null, Action dispose = null)
        {
            _logger.LogDebug("Initializing event source host");
            _start = start;
            _stop = stop;
            _run = run;
            _dispose = dispose;
            
            IsInitialized = true;
        }

        public bool IsInitialized { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_start != null)
            {
                return _start();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_stop != null)
            {
                return _stop();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _dispose?.Invoke();
        }
    }


    public interface IContinuousHostedService
    {
        Task StartAsync(CancellationToken stoppingToken);
        Task StopAsync(CancellationToken stoppingToken);
        void Dispose();
    }
    
    
    public class FileEventSource 
    {
        private readonly ILogger<FileEventSource> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private  FileSystemWatcher _fileSystemWatcher; 
        
        public FileEventSource(ILogger<FileEventSource> logger, ICloudEventPublisher cloudEventPublisher)
        {
            _logger = logger;
            _cloudEventPublisher = cloudEventPublisher;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _fileSystemWatcher = new FileSystemWatcher(@"c:\temp\eventf") { IncludeSubdirectories = true };

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
            _logger.LogInformation("Timed Hosted Service is stopping.");
            _fileSystemWatcher.EnableRaisingEvents = false;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }
}

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.Files;

namespace Weikio.EventFramework.Plugins.Files
{
    public class FileEventSource 
    {
        private readonly ILogger<FileEventSource> _logger;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private  FileSystemWatcher _fileSystemWatcher; 
        
        public string Folder { get; set; }
        public string Filter { get; set; }
        
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

    // public class TextFileContentEventSource: IHostedService, IDisposable
    // {
    //     private readonly ILogger<TextFileContentEventSource> _logger;
    //     private readonly ICloudEventPublisher _cloudEventPublisher;
    //     private Timer _timer;
    //     private List<string> _allLines = new List<string>();
    //
    //     // private object _currentState;
    //     
    //     public TextFileContentEventSource(ILogger<TextFileContentEventSource> logger, ICloudEventPublisher cloudEventPublisher)
    //     {
    //         _logger = logger;
    //         _cloudEventPublisher = cloudEventPublisher;
    //     }
    //
    //     private void DoWork(object state)
    //     {
    //         var lines = File.ReadLines(@"c:\temp\contentfile.txt").ToList();
    //
    //         var myState = (MyState) state;
    //         
    //         var currentState = JToken.FromObject(myState.Data);
    //         var newState = JToken.FromObject(lines);
    //         
    //         var diff = new JsonDiffPatch();
    //         var res = diff.Diff(currentState, newState);
    //
    //         if (res?.Any() != true)
    //         {
    //             return;
    //         }
    //         
    //         if (lines.Count <= _allLines.Count)
    //         {
    //             return;
    //         }
    //
    //         var newLines = lines.Skip(_allLines.Count).ToList();
    //
    //         var result = new NewLinesAddedEvent(newLines);
    //
    //         myState.Data = lines;
    //         // state = lines;
    //
    //         _cloudEventPublisher.Publish(result);
    //     }
    //
    //     public class MyState
    //     {
    //         public object Data { get; set; }
    //     }
    //     
    //     public Task StartAsync(CancellationToken stoppingToken)
    //     {
    //         var startingState = File.ReadLines(@"c:\temp\contentfile.txt").ToList();
    //
    //         var myState = new MyState() { Data = startingState };
    //         
    //         // _timer = new Timer(DoWork, myState, TimeSpan.Zero, 
    //         //     TimeSpan.FromSeconds(15));
    //         //
    //         return Task.CompletedTask;
    //         //
    //         // _fileSystemWatcher = new FileSystemWatcher(@"c:\temp\eventf") { IncludeSubdirectories = true };
    //         //
    //         // _fileSystemWatcher.Created += (sender, args) =>
    //         // {
    //         //     _cloudEventPublisher.Publish(new FileCreatedEvent(args.Name, args.FullPath));
    //         // };
    //         //
    //         // _fileSystemWatcher.Deleted += (sender, args) =>
    //         // {
    //         //     _cloudEventPublisher.Publish(new FileDeletedEvent(args.Name, args.FullPath));
    //         // };
    //         //
    //         // _fileSystemWatcher.Changed += (sender, args) =>
    //         // {
    //         // };
    //         //
    //         // _fileSystemWatcher.Renamed += (sender, args) =>
    //         // {
    //         //     _cloudEventPublisher.Publish(new FileRenamed(args.Name, args.FullPath, args.OldName, args.OldFullPath));
    //         // };
    //         //
    //         // _fileSystemWatcher.EnableRaisingEvents = true;
    //
    //         return Task.CompletedTask;
    //     }
    //
    //     public Task StopAsync(CancellationToken stoppingToken)
    //     {
    //         _logger.LogInformation("Timed Hosted Service is stopping.");
    //         _timer?.Change(Timeout.Infinite, 0);
    //
    //         return Task.CompletedTask;
    //     }
    //
    //     public void Dispose()
    //     {
    //         _timer?.Dispose();
    //     }
    // }


}

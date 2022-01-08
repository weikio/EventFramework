using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.Files
{
    [DisplayName("FileCloudEventSource")]
    public class FileCloudEventSource : IHostedService
    {
        private readonly ILogger<FileCloudEventSource> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly EventSourceInstanceContext _instanceContext;
        private readonly FileCloudEventSourceConfiguration _configuration;
        private FileSystemWatcher _fileSystemWatcher;
        private string _processingFolder;
        private CloudEventsChannel _processingChannel;

        public FileCloudEventSource(ILogger<FileCloudEventSource> logger, IServiceProvider serviceProvider,
            ICloudEventPublisher cloudEventPublisher, EventSourceInstanceContext instanceContext,
            FileCloudEventSourceConfiguration configuration = null)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cloudEventPublisher = cloudEventPublisher;
            _instanceContext = instanceContext;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_configuration == null)
            {
                throw new Exception("Configuration is required");
            }

            if (string.IsNullOrWhiteSpace(_configuration.Folder))
            {
                throw new Exception($"Missing mandatory configuration parameter {nameof(_configuration.Folder)}");
            }

            Directory.CreateDirectory(_configuration.Folder);

            _processingFolder = _configuration.GetProcessFolder(_instanceContext.EventSourceInstanceId, _configuration);
            Directory.CreateDirectory(_processingFolder);

            await CreateChannel();
            
            var filesInProcessing = new DirectoryInfo(_processingFolder).GetFiles("*.*", SearchOption.AllDirectories).OrderBy(x => x.CreationTime);

            foreach (var fileInfo in filesInProcessing)
            {
                await _processingChannel.Send(new FileFound(fileInfo.FullName));
            }

            var filesWaitingProcessing = new DirectoryInfo(_configuration.Folder)
                .GetFiles("*.*", _configuration.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).OrderBy(x => x.CreationTime);

            foreach (var fileInfo in filesWaitingProcessing)
            {
                await _processingChannel.Send(new FileFound(fileInfo.FullName));
            }

            _logger.LogInformation("Starting file cloud event source. Source folder: {Source}, processing folder: {ProcessingFolder}",
                _configuration.Folder, _processingFolder);

            _fileSystemWatcher =
                new FileSystemWatcher(_configuration.Folder, _configuration.Filter) { IncludeSubdirectories = _configuration.IncludeSubfolders };

            _fileSystemWatcher.Created += async (sender, args) =>
            {
                try
                {
                    await _processingChannel.Send(new FileFound(args.FullPath));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to publish new CloudEvent from path {FullPath}. Make sure file's content is JSON-formatted CloudEvent",
                        args.FullPath);
                }
            };

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
            _processingChannel.Dispose();
        }

        private async Task CreateChannel()
        {
            // We want a queue-like processing for all the files. Files are handled one by one using a local channel
            
            _processingChannel = await CloudEventsChannelBuilder.From(_instanceContext.EventSourceInstanceId + "_innerchannel")
                .Endpoint(async ev =>
                {
                    var filePath = ev.To<FileFound>().Object.Path;
                    await ProcessFile(filePath);
                })
                .Build(_serviceProvider);
        }

        private async Task ProcessFile(string sourceFilePath)
        {
            _logger.LogDebug("Processing new file {FilePath}", sourceFilePath);
            var sourceFileName = Path.GetFileName(sourceFilePath);
            var processingFilePath = Path.Combine(_processingFolder, sourceFileName);

            var parseError = false;
            var ok = false;
            CloudEvent cloudEvent = null;

            try
            {
                File.Move(sourceFilePath, processingFilePath);

                using (var file = File.Open(processingFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    using (var streamReader = new StreamReader(file))
                    {
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            try
                            {
                                var jObject = await JObject.LoadAsync(jsonReader);

                                var cloudEventFormatter = new JsonEventFormatter();
                                cloudEvent = cloudEventFormatter.DecodeJObject(jObject);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to parse CloudEvent from file {File}. Wrong format?", sourceFileName);

                                parseError = true;

                                throw;
                            }

                            _logger.LogDebug("Publishing CloudEvent from file {FilePath}", sourceFilePath);

                            await _cloudEventPublisher.Publish(cloudEvent);
                            ok = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process file from path {Path}", sourceFilePath);

                if (parseError)
                {
                    var errorFolder = _configuration.GetErrorFolder(_instanceContext.EventSourceInstanceId, _configuration);
                    Directory.CreateDirectory(errorFolder);

                    var errorFileName = Path.Combine(errorFolder, sourceFileName);
                    File.Move(processingFilePath, errorFileName);
                    
                    _logger.LogDebug("Moved failed CloudEvent file to {ErrorPath}", errorFileName);
                }
            }

            if (_configuration.Archive && ok && cloudEvent != null)
            {
                var archiveFolder = _configuration.GetArchiveFolder(_instanceContext.EventSourceInstanceId, _configuration, cloudEvent);
                Directory.CreateDirectory(archiveFolder);

                var archiveFileName = Path.Combine(archiveFolder, cloudEvent.Id + ".json");

                File.Move(processingFilePath, archiveFileName);
                
                _logger.LogDebug("Archived CloudEvent file to {ArchiveFile}", archiveFileName);
            }
        }
        

        internal class FileFound
        {
            public FileFound()
            {
            }

            public FileFound(string path)
            {
                Path = path;
            }

            internal string Path { get; set; }
        }
    }
}

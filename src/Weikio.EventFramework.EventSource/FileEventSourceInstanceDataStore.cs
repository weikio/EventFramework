using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class FileEventSourceInstanceDataStore : IEventSourceInstanceDataStore, IPersistableEventSourceInstanceDataStore
    {
        private readonly IOptions<FileEventSourceInstanceDataStoreOptions> _options;
        private readonly ILogger<FileEventSourceInstanceDataStore> _logger;
        private bool _pathLogged = false;
        
        public FileEventSourceInstanceDataStore(IOptions<FileEventSourceInstanceDataStoreOptions> options, ILogger<FileEventSourceInstanceDataStore> logger)
        {
            _options = options;
            _logger = logger;
        }

        private string GetPath()
        {
            try
            {
                var result = _options.Value.GetRootPath(EventSourceInstance);

                if (_pathLogged == false)
                {
                    _logger.LogDebug("Using path {FolderPath} as state persistence for for Event Source Instance {EventSourceInstanceId}. Making sure the path exists before proceeding", result, EventSourceInstance?.Id);
                    _pathLogged = true;
                }

                // TODO: Handle errors: Access rights / invalid directory name etc.
                Directory.CreateDirectory(result);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get path for storing event source state Event Source Instance {EventSourceInstanceId}", EventSourceInstance?.Id);

                try
                {
                    var filePath = _options.Value.GetRootPath(EventSourceInstance);
                    _logger.LogError("Path was {FilePath}. Make sure you have permission to write to the directory", filePath);
                }
                catch (Exception)
                {
                    // ignored
                }

                throw;
            }
        }

        public EventSourceInstance EventSourceInstance { get; set; }
        public Type StateType { get; set; }

        public Task<bool> HasRun()
        {
            try
            {
                var path = GetPath();

                var hasRun = File.Exists(Path.Combine(path, "firstrun.txt"));

                return Task.FromResult(hasRun);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to check if Event Source Instance {EventSourceInstanceId} has been run previously", EventSourceInstance?.Id);

                throw;
            }
        }

        public Task<dynamic> LoadState()
        {
            try
            {
                var path = GetPath();

                var stateFile = Path.Combine(path, "state.json");

                if (File.Exists(stateFile) == false)
                {
                    return Task.FromResult<object>(null);
                }

                using var file = File.OpenText(stateFile);

                var serializer = new JsonSerializer();
                var result = serializer.Deserialize(file, StateType);

                var taskResult = Task.FromResult(result);

                return taskResult;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Loading state failed for Event Source Instance {EventSourceInstanceId}", EventSourceInstance?.Id);

                throw;
            }
        }

        public async Task Save(dynamic updatedState)
        {
            try
            {
                var hasRun = await HasRun();
                var path = GetPath();

                if (hasRun == false)
                {
                    await File.WriteAllTextAsync(Path.Combine(path, "firstrun.txt"), DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                }

                var stateFile = Path.Combine(path, "state.json");
                await using var file = File.CreateText(stateFile);

                var serializer = new JsonSerializer();
                serializer.Serialize(file, updatedState);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Saving state failed for Event Source Instance {EventSourceInstanceId}", EventSourceInstance?.Id);

                throw;
            }
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class FileEventSourceInstanceDataStore : IEventSourceInstanceDataStore, IPersistableEventSourceInstanceDataStore
    {
        private readonly IOptions<FileEventSourceInstanceDataStoreOptions> _options;

        public FileEventSourceInstanceDataStore(IOptions<FileEventSourceInstanceDataStoreOptions> options)
        {
            _options = options;
        }

        private string GetPath()
        {
            var result = _options.Value.GetRootPath(EventSourceInstance);
            
            // TODO: Handle errors: Access rights / invalid directory name etc.
            Directory.CreateDirectory(result);

            return result;
        }

        public EventSourceInstance EventSourceInstance { get; set; }
        public Type StateType { get; set; }

        public Task<bool> HasRun()
        {
            var path = GetPath();

            var hasRun = File.Exists(Path.Combine(path, "firstrun.txt"));

            return Task.FromResult(hasRun);
        }

        public Task<dynamic> LoadState()
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

        public async Task Save(dynamic updatedState)
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
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class PollingScheduleService : List<PollingSchedule>
    {
        private readonly ILogger<PollingScheduleService> _logger;

        public PollingScheduleService(IEnumerable<PollingSchedule> jobSchedules, ILogger<PollingScheduleService> logger)
        {
            AddRange(jobSchedules);
            _logger = logger;
        }
    }

    public class FileEventSourceInstanceDataStore : IEventSourceInstanceDataStore, IPersistableEventSourceInstanceDataStore
    {
        public FileEventSourceInstanceDataStore(string eventSourceInstanceId)
        {
            EventSourceInstanceId = eventSourceInstanceId;
        }

        private string GetPath()
        {
            // ReSharper disable once PossibleNullReferenceException
            var entryAssembly = Assembly.GetEntryAssembly().GetName().Name ?? "";
            var result = Path.Combine(Path.GetTempPath(), "eventframework", entryAssembly, EventSourceInstanceId);

            // TODO: Handle errors: Access rights / invalid directory name etc.
            Directory.CreateDirectory(result);

            return result;
        }

        public string EventSourceInstanceId { get; }

        public Task<bool> HasRun()
        {
            var path = GetPath();

            var hasRun = File.Exists(Path.Combine(path, "firstrun.txt"));

            return Task.FromResult(hasRun);
        }

        public async Task<string> LoadState()
        {
            var path = GetPath();

            var stateFile = Path.Combine(path, "state.json");

            if (File.Exists(stateFile))
            {
                return await File.ReadAllTextAsync(stateFile);
            }

            return null;
        }

        public async Task Save(string updatedState)
        {
            var hasRun = await HasRun();
            var path = GetPath();

            if (hasRun == false)
            {
                await File.WriteAllTextAsync(Path.Combine(path, "firstrun.txt"), DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
            }

            await File.WriteAllTextAsync(Path.Combine(path, "state.json"), updatedState);
        }
    }

    public class InMemoryEventSourceInstanceDataStore : IEventSourceInstanceDataStore
    {
        private string _state = null;
        private bool _hasRun = false;
        public string EventSourceInstanceId { get; }

        public InMemoryEventSourceInstanceDataStore(string eventSourceInstanceId)
        {
            EventSourceInstanceId = eventSourceInstanceId;
        }

        public Task<bool> HasRun()
        {
            return Task.FromResult(_hasRun);
        }

        public Task<string> LoadState()
        {
            return Task.FromResult(_state);
        }

        public Task Save(string updatedState)
        {
            _state = updatedState;
            _hasRun = true;

            return Task.CompletedTask;
        }
    }

    internal class DefaultEventSourceInstanceStorageFactory : IEventSourceInstanceStorageFactory
    {
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;

        private readonly ConcurrentDictionary<string, IEventSourceInstanceDataStore> _dataStores =
            new ConcurrentDictionary<string, IEventSourceInstanceDataStore>();

        public DefaultEventSourceInstanceStorageFactory(IEventSourceInstanceManager eventSourceInstanceManager)
        {
            _eventSourceInstanceManager = eventSourceInstanceManager;
        }

        public Task<IEventSourceInstanceDataStore> GetStorage(string eventSourceInstanceId)
        {
            var result = _dataStores.GetOrAdd(eventSourceInstanceId, s =>
            {
                var esInstance = _eventSourceInstanceManager.Get(eventSourceInstanceId);

                if (esInstance.Options.PersistState)
                {
                    return new FileEventSourceInstanceDataStore(eventSourceInstanceId);
                }

                return new InMemoryEventSourceInstanceDataStore(eventSourceInstanceId);
            });

            return Task.FromResult(result);
        }
    }
}

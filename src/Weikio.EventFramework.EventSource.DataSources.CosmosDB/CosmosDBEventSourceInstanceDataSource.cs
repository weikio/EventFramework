using System;
using System.Threading;
using System.Threading.Tasks;
using Adafy.Candy.CosmosDB.Initialization;
using Newtonsoft.Json;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
    public class CosmosDBEventSourceInstanceDataSource : IPersistableEventSourceInstanceDataStore
    {
        private readonly StateRepository _stateRepository;
        private readonly CosmosDbInitializor _initializor;
        private bool _initialized = false;

        public CosmosDBEventSourceInstanceDataSource(StateRepository stateRepository, CosmosDbInitializor initializor)
        {
            _stateRepository = stateRepository;
            _initializor = initializor;
        }

        public EventSourceInstance EventSourceInstance { get; set; }
        public Type StateType { get; set; }

        public async Task<bool> HasRun()
        {
            if (_initialized == false)
            {
                await Initialize();
            }

            var doc = await _stateRepository.GetByInstanceId(EventSourceInstance.Id);

            return doc != null;
        }

        public async Task<dynamic> LoadState()
        {
            try
            {
                if (_initialized == false)
                {
                    await Initialize();
                }

                var doc = await _stateRepository.GetByInstanceId(EventSourceInstance.Id);

                if (doc == null)
                {
                    return null;
                }

                var obj = JsonConvert.DeserializeObject(doc.State, StateType);

                return obj;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task Initialize()
        {
            await _initializor.Initialize(new CancellationToken());

            _initialized = true;
        }

        public async Task Save(dynamic updatedState)
        {
            if (_initialized == false)
            {
                await Initialize();
            }

            var json = JsonConvert.SerializeObject(updatedState, Formatting.Indented);

            var doc = await _stateRepository.GetByInstanceId(EventSourceInstance.Id);

            if (doc == null)
            {
                doc = new StateEntity { State = json, EventSourceInstanceId = EventSourceInstance.Id };

                await _stateRepository.Insert(doc);

                return;
            }

            doc.State = json;
            await _stateRepository.Replace(doc);
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adafy.Candy.CosmosDB;
using Adafy.Candy.CosmosDB.Initialization;
using Adafy.Candy.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
    public class CosmosDBEventSourceInstanceDataSource : IEventSourceInstanceDataStore
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

    public class StateRepository : RepositoryBase<StateEntity>
    {
        public StateRepository(IDocumentWrapperService<StateEntity> db, ILogger<StateRepository> logger) : base(db, logger)
        {
        }

        public async Task<StateEntity> GetByInstanceId(string eventSourceInstanceId)
        {
            var query = Db.CreateQuery();

            query = query.Where(x => x.EventSourceInstanceId == eventSourceInstanceId);

            var queryResult = await Db.Find(query);

            var result = queryResult.FirstOrDefault();

            return result;
        }
    }

    public class StateEntity : Entity
    {
        public string EventSourceInstanceId { get; set; }
        public string State { get; set; }
    }
}

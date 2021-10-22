using System.Linq;
using System.Threading.Tasks;
using Adafy.Candy.CosmosDB;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
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
}
using System;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.EventSource.CosmosDB
{
    [EventType("CosmosDB.Change")]
    public class CosmosDBChange
    {
        public DateTime TimeNoticedUtc { get; set; }
        public JToken Change { get; set; }

        public CosmosDBChange()
        {
        }

        public CosmosDBChange(DateTime timeNoticedUtc, JToken change)
        {
            TimeNoticedUtc = timeNoticedUtc;
            Change = change;
        }
    }
}

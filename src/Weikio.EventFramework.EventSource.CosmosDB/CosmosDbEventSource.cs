using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.EventSource.CosmosDB
{
    public class CosmosDbEventSource
    {
        private readonly CosmosDBEventSourceConfiguration _configuration;
        private readonly ILogger<CosmosDbEventSource> _logger;

        public CosmosDbEventSource(CosmosDBEventSourceConfiguration configuration, ILogger<CosmosDbEventSource> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async IAsyncEnumerable<CosmosDBChange> CheckForChanges([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (_configuration == null || string.IsNullOrWhiteSpace(_configuration?.ConnectionString) || string.IsNullOrWhiteSpace(_configuration.Database) ||
                string.IsNullOrWhiteSpace(_configuration.Container))
            {
                _logger.LogError("Configuration is missing required information. Connection string, database and container required");

                throw new ArgumentNullException(nameof(_configuration), "Missing required configuration");
            }
            
            var cosmosClient =
                new CosmosClient(_configuration.ConnectionString);

            var db = cosmosClient.GetDatabase(_configuration.Database);
            
            _logger.LogInformation("Starting CosmosDB event source by listening to change feed of Database {Db}/Container {Container}", _configuration.Database, _configuration.Container);
            
            var iterator = db.GetContainer(_configuration.Container)
                .GetChangeFeedStreamIterator(ChangeFeedStartFrom.Now(), ChangeFeedMode.Incremental, new ChangeFeedRequestOptions());

            while (iterator.HasMoreResults )
            {
                using (var response = await iterator.ReadNextAsync(cancellationToken))
                {
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        _logger.LogDebug("No changes found, waiting for the delay set in configuration");
                        await Task.Delay(TimeSpan.FromSeconds(_configuration.PollingDelayInSeconds), cancellationToken);
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("Changes detected");

                        var serializer = new JsonSerializer();

                        using (var sr = new StreamReader(response.Content))
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            var change =  (JObject) serializer.Deserialize(jsonTextReader);

                            if (change != null)
                            {
                                var documents = (JArray)change["Documents"];
                                var changeCount = 0;
                                if (documents != null)
                                {
                                    foreach (var document in documents)
                                    {
                                        yield return new CosmosDBChange(DateTime.UtcNow, document);

                                        changeCount += 1;
                                    }
                                    
                                    _logger.LogDebug("Reported {ChangeCount} changes", changeCount);
                                }
                            }
                        }
                    }
                }
            }
            
            _logger.LogInformation("Stopping CosmosDB event source");
        }
    }
}

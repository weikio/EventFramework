using System;
using System.Linq;
using Adafy.Candy.CosmosDB;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDbStateStore(this IServiceCollection services, Action<CosmosDBEventSourceInstanceDataSourceOptions> options)
        {
            var opt = new CosmosDBEventSourceInstanceDataSourceOptions();
            options?.Invoke(opt);

            services.AddCosmosDb(settings =>
            {
                settings.CollectionId = opt.CollectionId;
                settings.DatabaseId = opt.DatabaseId;
                settings.DocumentDbKey = opt.DocumentDbKey;
                settings.DocumentDbUri = opt.DocumentDbUri;
            });

            services.AddTransient<StateRepository>();
            services.AddTransient<CosmosDBEventSourceInstanceDataSource>();

            if (opt.IsDefaultStateStore)
            {
                var currentDefault = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IPersistableEventSourceInstanceDataStore));

                if (currentDefault != null)
                {
                    services.Remove(currentDefault);
                }

                services.AddTransient<IPersistableEventSourceInstanceDataStore, CosmosDBEventSourceInstanceDataSource>();
            }
            
            return services;
        }
    }
}
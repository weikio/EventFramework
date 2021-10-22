using System;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.EventSource.DataSources.CosmosDB
{
    public static class IEventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddCosmosDbStateStore(this IEventFrameworkBuilder builder,
            Action<CosmosDBEventSourceInstanceDataSourceOptions> options)
        {
            var services = builder.Services;

            services.AddCosmosDbStateStore(options);

            return builder;
        }
    }
}
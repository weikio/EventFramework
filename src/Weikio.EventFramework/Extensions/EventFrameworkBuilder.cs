using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Extensions
{
    public class EventFrameworkBuilder : IEventFrameworkBuilder
    {
        public EventFrameworkBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}

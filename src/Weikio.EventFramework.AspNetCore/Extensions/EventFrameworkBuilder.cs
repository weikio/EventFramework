﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.AspNetCore.Extensions
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

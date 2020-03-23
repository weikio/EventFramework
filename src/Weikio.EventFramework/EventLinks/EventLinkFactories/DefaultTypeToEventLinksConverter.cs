﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventLinks.EventLinkFactories
{
    public class DefaultTypeToEventLinksConverter : ITypeToEventLinksConverter
    {
        private readonly IEnumerable<ITypeToHandlers> _typeToHandlers;

        public DefaultTypeToEventLinksConverter(IEnumerable<ITypeToHandlers> typeToHandlers)
        {
            _typeToHandlers = typeToHandlers;
        }

        public List<EventLink> Create(IServiceProvider provider, Type handlerType, Func<CloudEvent, Task<bool>> canHandle, MulticastDelegate configure)
        {
            var result = new List<EventLink>();

            var handlerMethods = _typeToHandlers.OrderBy(x => x.Priority).Select(x => x.GetHandlerMethods(handlerType)).ToList();
            
            var addedMethods = new List<MethodInfo>();

            foreach (var suppotedHandler in handlerMethods)
            {
                foreach (var supportedHandler in suppotedHandler.Item1)
                {
                    if (addedMethods.Contains(supportedHandler.Handler))
                    {
                        continue;
                    }

                    var handler = provider.GetRequiredService(handlerType);

                    configure?.DynamicInvoke(handler);

                    var runner = provider.GetRequiredService<IEventLinkRunner>();

                    runner.Initialize(handler, supportedHandler.Handler, suppotedHandler.Item2,
                        supportedHandler.Criteria, canHandle, supportedHandler.Guard);

                    var link = new EventLink(runner.CanHandle, runner.Handle);

                    result.Add(link);
                    addedMethods.Add(supportedHandler.Handler);
                }
            }
            
            return result;
        }
    }
}

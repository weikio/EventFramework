using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsChannelBuilder : IChannelBuilder
    {
        private CloudEventsChannelOptions _options;
        private List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>> _components = new List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>>();
        private List<Func<ComponentFactoryContext, Task<CloudEventsEndpoint>>> _endpoints = new List<Func<ComponentFactoryContext, Task<CloudEventsEndpoint>>>();

        public static CloudEventsChannelBuilder From(string channelName = ChannelName.Default)
        {
            return From(new CloudEventsChannelOptions() { Name = channelName });
        }

        public CloudEventsChannelBuilder WithName(string channelName)
        {
            _options.Name = channelName;

            return this;
        }
        
        public CloudEventsChannelBuilder Component(Func<CloudEvent, CloudEvent> func, Predicate<CloudEvent> predicate = null)
        {
            var cloudEventsComponent = new CloudEventsComponent(func, predicate);

            Task<CloudEventsComponent> Get(ComponentFactoryContext context)
            {
                return Task.FromResult(cloudEventsComponent);
            }

            return Component(Get);
        } 
        
        public CloudEventsChannelBuilder Component(Func<CloudEvent, Task<CloudEvent>> func, Predicate<CloudEvent> predicate = null)
        {
            var cloudEventsComponent = new CloudEventsComponent(func, predicate);

            Task<CloudEventsComponent> Get(ComponentFactoryContext context)
            {
                return Task.FromResult(cloudEventsComponent);
            }

            return Component(Get);
        } 
        
        public CloudEventsChannelBuilder Component(Func<ComponentFactoryContext, Task<CloudEventsComponent>> componentBuilder)
        {
            _components.Add(componentBuilder);

            return this;
        }        
        
        public CloudEventsChannelBuilder Component(IComponentBuilder componentBuilder)
        {
            _components.Add(componentBuilder.Build);

            return this;
        }   
        
        public CloudEventsChannelBuilder Endpoint(Func<ComponentFactoryContext, Task<CloudEventsEndpoint>> endpointBuilder)
        {
            _endpoints.Add(endpointBuilder);

            return this;
        }     
        
        public CloudEventsChannelBuilder Endpoint(Func<CloudEvent, Task> func, Predicate<CloudEvent> predicate = null)
        {
            var endpoint = new CloudEventsEndpoint(func, predicate);
            
            Task<CloudEventsEndpoint> Get(ComponentFactoryContext context)
            {
                return Task.FromResult(endpoint);
            }
            
            _endpoints.Add(Get);

            return this;
        }  
        
        public static CloudEventsChannelBuilder From(CloudEventsChannelOptions options)
        {
            var builder = new CloudEventsChannelBuilder();
            builder._options = options;

            return builder;
        }

        public IChannel Create(string channelName = ChannelName.Default)
        {
            return new CloudEventsChannel(new CloudEventsChannelOptions() { Name = channelName });
        }

        public IChannel Create(CloudEventsChannelOptions options)
        {
            return new CloudEventsChannel(options);
        }

        public async Task<IChannel> Build(IServiceProvider serviceProvider)
        {
            for (var index = 0; index < _components.Count; index++)
            {
                var componentFactory = _components[index];
                var context = new ComponentFactoryContext(serviceProvider, index, _options.Name);

                var component = await componentFactory(context);
                _options.Components.Add(component);
            }
            
            for (var index = 0; index < _endpoints.Count; index++)
            {
                var endpointFactory = _endpoints[index];
                var context = new ComponentFactoryContext(serviceProvider, index, _options.Name);

                var component = await endpointFactory(context);
                _options.Endpoints.Add(component);
            }

            return Create(_options);
        }
    }
}

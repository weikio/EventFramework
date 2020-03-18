using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventRoute
    {
        Task<bool> CanHandle(CloudEvent cloudEvent);
        Task<bool> Handle(CloudEvent cloudEvent);
    }

    public interface ICloudEventRoute<T> : ICloudEventRoute
    {
        Task<bool> CanHandle(CloudEvent<T> cloudEvent);
        Task<bool> Handle(CloudEvent<T> cloudEvent);
    }

    public class RoutingHandler
    {
        public string IncomingGatewayName;
        public string OutgoingGatewayName;
        public IServiceProvider ServiceProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        public Predicate<CloudEvent> Filter;
        public Func<CloudEvent, IServiceProvider, Task<CloudEvent>> OnRouting;

        public RoutingHandler(IServiceProvider serviceProvider, ICloudEventPublisher cloudEventPublisher, ICloudEventGatewayManager cloudEventGatewayManager)
        {
            ServiceProvider = serviceProvider;
            _cloudEventPublisher = cloudEventPublisher;
            _cloudEventGatewayManager = cloudEventGatewayManager;
        }
        
        public Task<bool> CanHandle(CloudEvent cloudEvent)
        {
            if (!string.Equals(cloudEvent.Gateway(), IncomingGatewayName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            if (Filter == null)
            {
                return Task.FromResult(true);
            }

            var result = Filter(cloudEvent);

            return Task.FromResult(result);
        }

        public async Task<bool> Handle(CloudEvent cloudEvent)
        {
            var newContext = cloudEvent;
            
            if (OnRouting != null)
            {
                newContext = await OnRouting(cloudEvent, ServiceProvider);
            }

            var gateway = _cloudEventGatewayManager.Get(OutgoingGatewayName);
            
            await _cloudEventPublisher.Publish(newContext, gateway.Name);

            return true;
        }
    }

    public class RouteCloudEventRoute : ICloudEventRoute
    {
        private readonly string _incomingGatewayName;
        private readonly string _outgoingGatewayName;
        private readonly IServiceProvider _serviceProvider;
        private readonly Predicate<CloudEvent> _filter;
        private readonly Func<CloudEvent, IServiceProvider, Task<CloudEvent>> _onRouting;

        public RouteCloudEventRoute(string incomingGatewayName, string outgoingGatewayName, IServiceProvider serviceProvider,  
            Predicate<CloudEvent> filter = null, Func<CloudEvent, IServiceProvider, Task<CloudEvent>> onRouting = null)
        {
            _incomingGatewayName = incomingGatewayName;
            _outgoingGatewayName = outgoingGatewayName;
            _serviceProvider = serviceProvider;
            _filter = filter;
            _onRouting = onRouting;
        }

        public Task<bool> CanHandle(CloudEvent cloudEvent)
        {
            if (!string.Equals(cloudEvent.Gateway(), _incomingGatewayName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            if (_filter == null)
            {
                return Task.FromResult(true);
            }

            var result = _filter(cloudEvent);

            return Task.FromResult(result);
        }

        public async Task<bool> Handle(CloudEvent cloudEvent)
        {
            var newContext = cloudEvent;
            
            if (_onRouting != null)
            {
                newContext = await _onRouting(cloudEvent, _serviceProvider);
            }

            var publisher = _serviceProvider.GetRequiredService<ICloudEventPublisher>();
            var gatewayManager = _serviceProvider.GetRequiredService<ICloudEventGatewayManager>();

            var gateway = gatewayManager.Get(_outgoingGatewayName);
            
            await publisher.Publish(newContext, gateway.Name);

            return true;
        }
    }
}

using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventRoute
    {
        Task<bool> CanHandle(ICloudEventContext cloudEvent);
        Task<bool> Handle(ICloudEventContext cloudEvent);
    }

    public interface ICloudEventRoute<T> : ICloudEventRoute
    {
        Task<bool> CanHandle(CloudEvent<T> cloudEvent);
        Task<bool> Handle(CloudEvent<T> cloudEvent);
    }

    public class RouteCloudEventRoute : ICloudEventRoute
    {
        private readonly string _incomingGatewayName;
        private readonly string _outgoingGatewayName;
        private readonly IServiceProvider _serviceProvider;
        private readonly Predicate<ICloudEventContext> _filter;
        private readonly Func<ICloudEventContext, IServiceProvider, Task<ICloudEventContext>> _onRouting;

        public RouteCloudEventRoute(string incomingGatewayName, string outgoingGatewayName, IServiceProvider serviceProvider,  
            Predicate<ICloudEventContext> filter = null, Func<ICloudEventContext, IServiceProvider, Task<ICloudEventContext>> onRouting = null)
        {
            _incomingGatewayName = incomingGatewayName;
            _outgoingGatewayName = outgoingGatewayName;
            _serviceProvider = serviceProvider;
            _filter = filter;
            _onRouting = onRouting;
        }

        public Task<bool> CanHandle(ICloudEventContext cloudEvent)
        {
            if (!string.Equals(cloudEvent.Gateway.Name, _incomingGatewayName, StringComparison.InvariantCultureIgnoreCase))
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

        public async Task<bool> Handle(ICloudEventContext cloudEvent)
        {
            var newContext = cloudEvent;
            
            if (_onRouting != null)
            {
                newContext = await _onRouting(cloudEvent, _serviceProvider);
            }

            var publisher = _serviceProvider.GetRequiredService<ICloudEventPublisher>();
            var gatewayManager = _serviceProvider.GetRequiredService<ICloudEventGatewayManager>();

            var gateway = gatewayManager.Get(_outgoingGatewayName);
            
            await publisher.Publish(newContext.CloudEvent, gateway.Name);

            return true;
        }
    }
}

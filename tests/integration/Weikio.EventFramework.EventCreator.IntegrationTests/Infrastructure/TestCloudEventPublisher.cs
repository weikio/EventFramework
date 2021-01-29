using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public class TestCloudEventPublisher : ICloudEventPublisher
    {
        public List<object> PublishedEvents = new List<object>();

        public TestCloudEventPublisher()
        {
        }

        public Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(cloudEvent);

            return Task.FromResult<CloudEvent>(cloudEvent);
        }

        public Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            PublishedEvents.AddRange(objects);

            return Task.FromResult(new List<CloudEvent>());
        }

        public Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(obj);

            return Task.FromResult(new CloudEvent("test", new Uri("http://localhost", UriKind.Absolute), Guid.NewGuid().ToString()));
        }
    }
    
    public class MyTestCloudEventPublisher : CloudEventPublisher
    {
        public static List<CloudEvent> PublishedEvents = new List<CloudEvent>();
        private CloudEventPublisherOptions _myOptions;
        private IServiceProvider _myServiceProvider;

        public MyTestCloudEventPublisher(ICloudEventGatewayManager gatewayManager, IOptions<CloudEventPublisherOptions> options, ICloudEventCreator cloudEventCreator, IServiceProvider serviceProvider) : base(gatewayManager, options, cloudEventCreator, serviceProvider)
        {
            _myOptions = options.Value;
            _myServiceProvider = serviceProvider;
        }
        
        public override async Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName)
        {
            if (cloudEvent == null)
            {
                throw new ArgumentNullException(nameof(cloudEvent));
            }

            if (string.IsNullOrEmpty(cloudEvent.Id))
            {
                cloudEvent.Id = Guid.NewGuid().ToString();
            }

            var beforePublish = _myOptions.OnBeforePublish;

            if (beforePublish != null)
            {
                cloudEvent = await beforePublish(_myServiceProvider, cloudEvent);
            }

            PublishedEvents.Add(cloudEvent);

            return cloudEvent;
        }
    }
    
    public class MyTestCloudEventPublisherFactory : ICloudEventPublisherFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MyTestCloudEventPublisherFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public CloudEventPublisher Create(Guid eventSourceInstanceId)
        {
            var gatewayManager = _serviceProvider.GetRequiredService<ICloudEventGatewayManager>();
            var cloudEventCreator = _serviceProvider.GetRequiredService<ICloudEventCreator>();

            var result = new MyTestCloudEventPublisher(gatewayManager, new OptionsWrapper<CloudEventPublisherOptions>(new CloudEventPublisherOptions()
                {
                    OnBeforePublish = (provider, cloudEvent) =>
                    {
                        var extension = new EventFrameworkEventSourceExtension(eventSourceInstanceId);
                        extension.Attach(cloudEvent);

                        return Task.FromResult(cloudEvent);
                    }
                }),
                
                cloudEventCreator, _serviceProvider);

            return result;
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public interface ICloudEventPublisher
    {
        Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default);

        Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default);
        
        Task<List<CloudEvent>> Publish(IEnumerable objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default);
    }
}

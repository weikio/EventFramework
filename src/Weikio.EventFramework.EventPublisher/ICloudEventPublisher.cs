using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public interface ICloudEventPublisher
    {
        Task Publish(object obj, string channelName = null);
    }
}

using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventCreator
{
    public interface ICloudEventCreator
    {
        CloudEvent CreateCloudEvent(object obj, string eventTypeName = null, string id = null, Uri source = null, ICloudEventExtension[] extensions = null,
            string subject = null);
        
        IEnumerable<CloudEvent> CreateCloudEvents(IEnumerable<object> objects, string eventTypeName = null, string id = null, Uri source = null, ICloudEventExtension[] extensions = null,
            string subject = null);
    }
}

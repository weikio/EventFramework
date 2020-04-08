using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Publisher
{
    public interface ICloudEventCreator
    {
        CloudEvent CreateCloudEvent(object obj, string eventTypeName, string id, Uri source, ICloudEventExtension[] extensions = null,
            string subject = null);
    }
}

using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components
{
    public class AddExtensionComponent : CloudEventsComponent
    {
        public AddExtensionComponent(ICloudEventExtension extension, Predicate<CloudEvent> predicate = null)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            
            Func = ev =>
            {
                extension.Attach(ev);
                return Task.FromResult(ev);
            };

            if (predicate == null)
            {
                return;
            }

            Predicate = predicate;
        }
    }
}

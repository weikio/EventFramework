using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components
{
    public class AddExtensionComponent : CloudEventsComponent
    {
        public AddExtensionComponent(Func<CloudEvent, ICloudEventExtension> extensionFactory, Predicate<CloudEvent> predicate = null)
        {
            if (extensionFactory == null)
            {
                throw new ArgumentNullException(nameof(extensionFactory));
            }
            
            Func = ev =>
            {
                var extension = extensionFactory.Invoke(ev);
                extension.Attach(ev);
                return Task.FromResult(ev);
            };

            if (predicate == null)
            {
                predicate = ev => true;
            }

            Predicate = predicate;
        }
    }

    public class BranchComponent
    {
        
    }
}

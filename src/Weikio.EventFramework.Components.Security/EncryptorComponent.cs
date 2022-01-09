using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Security
{
    public class EncryptorComponent : CloudEventsComponent
    {
        private readonly EncryptorComponentOptions _configuration;
        private readonly Encryptor _encryptor;

        public EncryptorComponent(EncryptorComponentOptions configuration)
        {
            _configuration = configuration;
            _encryptor = new Encryptor(configuration.PublicKey, configuration.PublicKeyPath);

            Func = Encrypt;
        }

        public Task<CloudEvent> Encrypt(CloudEvent cloudEvent)
        {
            var json = cloudEvent.ToJson();
            var encyptedEvent = _encryptor.Encrypt(json);

            var keyExtension = new EncryptedKeyCloudEventExtension(encyptedEvent.EncryptedKey);
            var eventExtension = new EncryptedEventCloudEventExtension(encyptedEvent.EncryptedText);

            keyExtension.Attach(cloudEvent);
            eventExtension.Attach(cloudEvent);

            return Task.FromResult(cloudEvent);
        }
    }
}

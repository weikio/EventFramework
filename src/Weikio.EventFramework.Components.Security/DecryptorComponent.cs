using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Security
{
    public class DecryptorComponent : CloudEventsComponent
    {
        private readonly DecryptorComponentOptions _configuration;
        private readonly IChannelManager _channelManager;
        private readonly Decryptor _decryptor;

        public DecryptorComponent(DecryptorComponentOptions configuration, IChannelManager channelManager)
        {
            _configuration = configuration;
            _channelManager = channelManager;
            _decryptor = new Decryptor(configuration.PrivateKey, configuration.PrivateKeyPath);

            Func = Decrypt;
        }

        public async Task<CloudEvent> Decrypt(CloudEvent cloudEvent)
        {
            IChannel deadletter = null;

            if (!string.IsNullOrWhiteSpace(_configuration.DeadletterChannel))
            {
                deadletter = _channelManager.Get(_configuration.DeadletterChannel);
            }
            
            // Get the result from the decrypted attributes
            var encryptedKey = cloudEvent.GetAttributes().ContainsKey(_configuration.EncryptKeyAttributeName)
                ? cloudEvent.GetAttributes().FirstOrDefault(x => x.Key == _configuration.EncryptKeyAttributeName).Value.ToString()
                : "";

            var encryptedEvent = cloudEvent.GetAttributes().ContainsKey(_configuration.EncryptEventAttributeName)
                ? cloudEvent.GetAttributes().FirstOrDefault(x => x.Key == _configuration.EncryptEventAttributeName).Value.ToString()
                : "";

            if (string.IsNullOrWhiteSpace(encryptedKey) || string.IsNullOrWhiteSpace(encryptedEvent))
            {
                if (_configuration.AllowNonEncrypted)
                {
                    return cloudEvent;
                }

                if (deadletter != null)
                {
                    await deadletter.Send(cloudEvent);

                    return null;
                }                
            }

            try
            {
                var decryptedEventData = _decryptor.Decrypt(encryptedEvent, encryptedKey);
                
                // Validate data hasn't changed
                var data = cloudEvent.ToJObject()["data"].ToString();

                if (string.Equals(data, decryptedEventData))
                {
                    return cloudEvent;
                }

                throw new Exception("Data has changed");
            }
            catch (Exception)
            {
                if (deadletter != null)
                {
                    await deadletter.Send(cloudEvent);
                }  
                
                return null;
            }
        }
    }
}

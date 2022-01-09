namespace Weikio.EventFramework.Components.Security
{
    public class DecryptorComponentOptions
    {
        public string PrivateKey { get; set; }
        public string PrivateKeyPath { get; set; }
        public string DeadletterChannel { get; set; }
        public string EncryptKeyAttributeName { get; set; } = EncryptedKeyCloudEventExtension.EncryptedKeyExtension;
        public string EncryptEventAttributeName { get; set; } = EncryptedEventCloudEventExtension.EncryptedEventExtension;
        public bool AllowNonEncrypted { get; set; } = true;
    }
}
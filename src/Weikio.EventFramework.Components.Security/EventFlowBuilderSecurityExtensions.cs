using Weikio.EventFramework.Components.Security;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderSecurityExtensions
    {
        public static IEventFlowBuilder Decrypt(this IEventFlowBuilder builder, string privateKey = "", string privateKeyPath = "", string deadLetterChannel = "")
        {
            var options = new DecryptorComponentOptions() { PrivateKey = privateKey, PrivateKeyPath = privateKeyPath, DeadletterChannel = deadLetterChannel};

            var componentBuilder = new DecryptorComponentBuilder(options);

            builder.Component(componentBuilder);

            return builder;
        }
        
        public static IEventFlowBuilder Encrypt(this IEventFlowBuilder builder, string publicKey = "", string publicKeyPath = "")
        {
            var options = new EncryptorComponentOptions() { PublicKey = publicKey, PublicKeyPath = publicKeyPath };

            var componentBuilder = new EncryptorComponentBuilder(options);

            builder.Component(componentBuilder);

            return builder;
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Weikio.EventFramework.Components.Security
{
    public class Encryptor
    {
        private readonly string _publicKey;

        public Encryptor(string publicKey = "", string publicKeyPath = "")
        {
            _publicKey = publicKey;
            
            if (string.IsNullOrWhiteSpace(publicKeyPath))
            {
                return;
            }

            _publicKey = File.ReadAllText(publicKeyPath, Encoding.UTF8);
        }

        public (string EncryptedText, string EncryptedKey) Encrypt(string textToEncrypt)
        {
            var publicKeyString = _publicKey;

            var result = Encrypt(textToEncrypt, publicKeyString);

            return result;
        }

        private (string EncryptedText, string EncryptedKey) Encrypt(string textToEncrypt, string publicKeyString)
        {
            byte[] aesKeyBytes;
            string encryptedText;

            // First encrypt the result
            using (var aes = Aes.Create())
            {
                aesKeyBytes = aes.Key;
                var encryptor = aes.CreateEncryptor();

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                            swEncrypt.Write(textToEncrypt);
                        }

                        var encryptedBytes = msEncrypt.ToArray();
                        encryptedText = Convert.ToBase64String(encryptedBytes);
                    }
                }
            }

            // Then encrypt the key used to encrypt the result
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    rsa.FromXmlString(publicKeyString);
                    var encryptedData = rsa.Encrypt(aesKeyBytes, true);
                    var encryptedKey = Convert.ToBase64String(encryptedData);

                    // Return both
                    return (encryptedText, encryptedKey);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
    }
}

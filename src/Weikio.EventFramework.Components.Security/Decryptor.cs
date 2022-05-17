using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Weikio.EventFramework.Components.Security
{
    public class Decryptor
    {
        private readonly string _privateKey;

        public Decryptor(string privateKey = "", string privateKeyPath = "")
        {
            _privateKey = privateKey;
            
            if (string.IsNullOrWhiteSpace(privateKeyPath))
            {
                return;
            }

            _privateKey = File.ReadAllText(privateKeyPath, Encoding.UTF8);
        }

        public string Decrypt(string encryptedText, string encryptedKey)
        {
            var privateKeyString = _privateKey;

            // First decrypt the key using the private key
            byte[] decryptedKey;

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    rsa.FromXmlString(privateKeyString);

                    var keyBytes = Convert.FromBase64String(encryptedKey);
                    decryptedKey = rsa.Decrypt(keyBytes, true);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }

            string result = null;

            // Then decrypt the result using the decrypted key
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = decryptedKey;

                var base64Text = Convert.FromBase64String(encryptedText);

                using (var msDecrypt = new MemoryStream(base64Text))
                {
                    var iv = new byte[16];
                    msDecrypt.Read(iv, 0, 16);
                    aesAlg.IV = iv;

                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return result;
        }
    }
}

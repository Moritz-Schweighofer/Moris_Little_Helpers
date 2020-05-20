using System;
using System.Security.Cryptography;
using System.Text;

namespace Schweigm_NETCore_Helpers
{
    public static class RsaController
    {
        public static string Encrypt(string data, string publicKey)
        {
            try
            {
                var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(publicKey);
                var encoder = new UnicodeEncoding();
                var dataToEncrypt = encoder.GetBytes(data);

                var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false);
                var encryptedString = Convert.ToBase64String(encryptedByteArray);

                return encryptedString;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Error With Encrypting the Password: " + ex.Message);
                return "";
            }
        }

        public static string Decrypt(string data, string privateKey)
        {
            try
            {
                var rsa = new RSACryptoServiceProvider();
                var dataByte = Convert.FromBase64String(data);

                rsa.FromXmlString(privateKey);
                var decryptedByte = rsa.Decrypt(dataByte, false);
                var encoder = new UnicodeEncoding();

                return encoder.GetString(decryptedByte);
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Error With Decrypting the Password: " + ex.Message);
                return "";
            }
        }
    }
}
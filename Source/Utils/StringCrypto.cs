using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Ulee.Utils
{
    public class UlStringCrypto
    {
        private byte[] bytes;

        private DESCryptoServiceProvider provider;

        public UlStringCrypto(string key)
        {
            if (key.Length != 8)
            {
                throw new Exception("It must be 8 bytes for crypto key length!");
            }

            byte[] cryptoKey = ASCIIEncoding.ASCII.GetBytes(key);

            provider = new DESCryptoServiceProvider();
            provider.Key = cryptoKey;
            provider.IV = cryptoKey;

        }

        public string Encrypt(string str)
        {
            bytes = Encoding.UTF8.GetBytes(str.ToCharArray());

            MemoryStream memStream = new MemoryStream();
            CryptoStream encryptoStream = new CryptoStream(memStream, provider.CreateEncryptor(), CryptoStreamMode.Write);

            encryptoStream.Write(bytes, 0, bytes.Length);
            encryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memStream.ToArray());
        }

        public string Decrypt(string str)
        {
            bytes = Convert.FromBase64String(str);

            MemoryStream memStream = new MemoryStream();
            CryptoStream decryptoStream = new CryptoStream(memStream, provider.CreateDecryptor(), CryptoStreamMode.Write);

            decryptoStream.Write(bytes, 0, bytes.Length);
            decryptoStream.FlushFinalBlock();

            return Encoding.UTF8.GetString(memStream.GetBuffer());
        }
    }
}

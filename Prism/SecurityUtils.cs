using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MarksWebService
{
    public class SecurityUtils
    {
        private TripleDESCryptoServiceProvider TripleDes = new TripleDESCryptoServiceProvider();

        private byte[] TruncateHash(string key, int length)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

            // Hash the key.
            byte[] keyBytes = Encoding.Unicode.GetBytes(key);
            byte[] hash = sha1.ComputeHash(keyBytes);

            // Truncate or pad the hash.
            Array.Resize(ref hash, length);
            return hash;
        }

        public SecurityUtils(string keyText = "")
        {
            if (string.IsNullOrEmpty(keyText))
            {
                byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
                byte[] iv = { 65, 110, 68, 26, 69, 178, 200, 219 };
                TripleDes.Key = key;
                TripleDes.IV = iv;
            }
            else
            {
                TripleDes.Key = TruncateHash(keyText, TripleDes.KeySize / 8);
                TripleDes.IV = TruncateHash("", TripleDes.BlockSize / 8);
            }
        }

        public string Encrypt(string plaintext)
        {
            // Convert the plaintext string to a byte array.
            byte[] plaintextBytes = Encoding.Unicode.GetBytes(plaintext);

            // Create the stream.
            MemoryStream ms = new MemoryStream();
            // Create the encoder to write to the stream.
            CryptoStream encStream = new CryptoStream(ms,
                TripleDes.CreateEncryptor(),
                CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            encStream.Write(plaintextBytes, 0, plaintextBytes.Length);
            encStream.FlushFinalBlock();

            // Convert the encrypted stream to a printable string.
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string encryptedtext)
        {
            // Convert the encrypted text string to a byte array.
            byte[] encryptedBytes = Convert.FromBase64String(encryptedtext);

            // Create the stream.
            MemoryStream ms = new MemoryStream();
            // Create the decoder to write to the stream.
            CryptoStream decStream = new CryptoStream(ms,
                TripleDes.CreateDecryptor(),
                CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            decStream.FlushFinalBlock();

            // Convert the plaintext stream to a string.
            return Encoding.Unicode.GetString(ms.ToArray());
        }
    }
}
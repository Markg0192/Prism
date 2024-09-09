using Prism.ExternalService;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Prism
{
	public static class WebService
    {
        private static SecurityUtils _encoder = new SecurityUtils("Prism"); //The key given here is used for excryption/decryption of information passed between services and apps
        public static WebService1 _service;

        public static WebService1 SetupWebService(string key)
        {
            _service = new WebService1();
            _service.Url = @"https://webapps.severfield.com/CETExtWebService/ExternalService.asmx";

            AuthHeader soapHead = new AuthHeader();
            SecurityUtils secUtils = new SecurityUtils("Extd6L!u8nO1%qR7"); //This security is used for the Authentication header

            string userName = Environment.UserName;
            soapHead.Username = secUtils.Encrypt(userName);
            soapHead.ProgramName = secUtils.Encrypt("Prism");
            soapHead.ProgramVersion = secUtils.Encrypt(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            soapHead.DomainName = secUtils.Encrypt(Environment.UserDomainName);
            soapHead.UniqueUserId = secUtils.Encrypt(key);
            soapHead.MotherBoardId = secUtils.Encrypt(GetMotherboardID());

            _service.AuthHeaderValue = soapHead;

            if (_service.HelloWorld() != "Hello World")
            {
                MessageBox.Show("Failed to connect to the web service, please ensure internet connection. If the problem persists, contact help.");
                Environment.Exit(1);
            }
            return _service;  
        }

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

            public string[] Encrypt(string[] plainTextArray)
            {
                return plainTextArray.ToList().Select(s => Encrypt(s)).ToArray();
            }

            public string[] Decrypt(string[] encryptedTextArray)
            {
                return encryptedTextArray.ToList().Select(s => Decrypt(s)).ToArray();
            }
        }

        public static string[] GetDirectories(int fileLocation, string additionalPath)
        {
            return _encoder.Decrypt(_service.GetDirectories(fileLocation, additionalPath));
        }

        public static string GetDirectoryName(int filePathLine, string additionalPath)
        {
            return _encoder.Decrypt(_service.GetDirectoryName(filePathLine, additionalPath));
        }

        public static string[] DirectoryGetFiles(int filePathLine, string searchPattern, string additionalString)
        {
            return _encoder.Decrypt(_service.DirectoryGetFiles(filePathLine, searchPattern, additionalString));
        }

        public static string[] ReadAllLinesIntoArray(int filePathLine, string additionalString)
        {
            return _encoder.Decrypt(_service.ReadAllLinesIntoArray(filePathLine, additionalString));
        }

        public static string ReadSpecificLine(int filePathLine, int lineToRead, string additionalString)
        {
            return _encoder.Decrypt(_service.ReadSpecificLine(filePathLine, lineToRead, additionalString));
        }

        public static void WriteToSpecificLine(int filePathLine, int lineToWriteTo, string content, string additionalPath)
        {
            string encryptedContent = _encoder.Encrypt(content);
            _service.WriteToSpecificLine(filePathLine, lineToWriteTo, encryptedContent, additionalPath);
        }

        public static void WriteAllLinesWithArray(int filePathLine, string[] content, string addditionalPath)
        {
            string[] encryptedContent = _encoder.Encrypt(content);
            _service.WriteAllLinesWithArray(filePathLine, encryptedContent, addditionalPath);
        }

        public static void CreateNewDirectory(int filePathLine, string additionalPath)
        {
            _service.CreateNewDirectory(filePathLine, additionalPath);
        }

        public static void WriteAppendStringsToFile(int filePathLine, string[] content, string additionalPath)
        {
            string[] encryptedContent = _encoder.Encrypt(content);
            _service.WriteAppendStringsToFile(filePathLine, encryptedContent, additionalPath);
        }

        public static void WriteAppendStringToFile(int filePathLine, string content, string additionalPath)
        {
            string encryptedContent = _encoder.Encrypt(content);
            _service.WriteAppendStringToFile(filePathLine, encryptedContent, additionalPath);
        }

        public static bool FileExists(string fileToCheck, int filePathLine, string additionalString)
        {
            string encryptedFileToCheck = _encoder.Encrypt(fileToCheck);
            return _service.FileExists(encryptedFileToCheck, filePathLine, additionalString);
        }

        private static string GetMotherboardID()
        {
            var managementObjects = new ManagementObjectSearcher("Select * From Win32_BaseBoard").Get();
            if (managementObjects.Count == 1) foreach (var mb in managementObjects) return mb["SerialNumber"].ToString();
            return Environment.MachineName;
        }
    }
}
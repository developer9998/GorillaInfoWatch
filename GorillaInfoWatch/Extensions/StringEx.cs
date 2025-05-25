using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GorillaNetworking;

namespace GorillaInfoWatch.Extensions
{
    public static class StringEx
    {
        private static readonly Dictionary<string, string> SanitizedNames = [];

        private static readonly string Key = "ShibaAspectAndTundraSmellLikeDog"; // still accurate and i can't think of anything better

        public static string NormalizeName(this string name)
        {
            if (SanitizedNames.ContainsKey(name)) return SanitizedNames[name];

            string original_name = name;

            if (GorillaComputer.instance.CheckAutoBanListForName(name))
            {
                name = new string(Array.FindAll(name.ToCharArray(), Utils.IsASCIILetterOrDigit)).LimitLength(12);
                name = (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) ? "<empty>" : name.ToUpper();
            }
            else
            {
                name = "<badname>"; // scoreboard handles reporting, no need to do it here, or in any mod for that matter
            }

            SanitizedNames.Add(original_name, name);
            return name;
        }

        public static string LimitLength(this string str, int length)
        {
            return (str.Length > length) ? str[..length] : str;
        }

        // https://github.com/KyleTheScientist/Bark/blob/3de171aca033d45f464a5120fb1932c9a0d2a3af/Extensions/StringExtensions.cs#L11
        public static string EncryptString(this string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        // https://github.com/KyleTheScientist/Bark/blob/3de171aca033d45f464a5120fb1932c9a0d2a3af/Extensions/StringExtensions.cs#L40
        public static string DecryptString(this string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            return streamReader.ReadToEnd();
        }
    }
}

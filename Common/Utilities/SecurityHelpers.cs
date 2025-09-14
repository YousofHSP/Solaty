using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Common.Utilities
{
    public static class SecurityHelpers
    {
        private static string _key = JwtSettings.EncryptKey;

        public static string GetSha256Hash(string input, string salt = "")
        {
            if (string.IsNullOrEmpty(input)) return "";
            var byteValue = Encoding.UTF8.GetBytes(input);
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(input + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        public static string EncryptAes(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = new byte[16]; // مقدار IV باید تصادفی باشد
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var inputBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptAes(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = new byte[16];
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var cipherBytes = Convert.FromBase64String(cipherText);
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static string GenerateSalt()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
using HelpDesk.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HelpDesk.Infrastructure.Security
{
    // AES-256-CBC with an HMAC-SHA256 tag (encrypt then MAC). Stored as "enc:v1:" + base64(iv + data + tag).
    // The two keys are derived from the configured secret, so the same value works across restarts and instances.
    public class AesSecretProtector : ISecretProtector
    {
        private const string Prefix = "enc:v1:";
        private readonly byte[] _encryptionKey;
        private readonly byte[] _macKey;

        public AesSecretProtector(string secret)
        {
            if (string.IsNullOrEmpty(secret) || secret.Length < 32)
            {
                throw new ArgumentException("The secrets key must have at least 32 characters.");
            }
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                _encryptionKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("helpdesk-encryption"));
                _macKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("helpdesk-mac"));
            }
        }

        public bool IsProtected(string stored)
        {
            return !string.IsNullOrEmpty(stored) && stored.StartsWith(Prefix);
        }

        public string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText) || IsProtected(plainText))
            {
                return plainText;
            }

            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.GenerateIV();
                byte[] encrypted;
                using (var encryptor = aes.CreateEncryptor())
                {
                    var data = Encoding.UTF8.GetBytes(plainText);
                    encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
                }
                var payload = aes.IV.Concat(encrypted).ToArray();
                var tag = Mac(payload);
                return Prefix + Convert.ToBase64String(payload.Concat(tag).ToArray());
            }
        }

        public string Unprotect(string stored)
        {
            if (!IsProtected(stored))
            {
                return stored;
            }

            var all = Convert.FromBase64String(stored.Substring(Prefix.Length));
            if (all.Length < 16 + 16 + 32)
            {
                throw new CryptographicException("The protected value is not valid.");
            }
            var payload = all.Take(all.Length - 32).ToArray();
            var tag = all.Skip(all.Length - 32).ToArray();
            if (!ConstantTimeEquals(Mac(payload), tag))
            {
                throw new CryptographicException("The protected value was altered or the key is different.");
            }

            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = payload.Take(16).ToArray();
                var cipher = payload.Skip(16).ToArray();
                using (var decryptor = aes.CreateDecryptor())
                {
                    return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(cipher, 0, cipher.Length));
                }
            }
        }

        private byte[] Mac(byte[] payload)
        {
            using (var hmac = new HMACSHA256(_macKey))
            {
                return hmac.ComputeHash(payload);
            }
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}

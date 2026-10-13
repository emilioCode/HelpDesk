using HelpDesk.Core.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace HelpDesk.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 11;

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
            {
                return false;
            }

            if (IsHashed(stored))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, stored);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // Legacy plain-text value, compared in constant time.
            var a = Encoding.UTF8.GetBytes(password);
            var b = Encoding.UTF8.GetBytes(stored);
            return a.Length == b.Length && ConstantTimeEquals(a, b);
        }

        public bool IsHashed(string stored)
        {
            return !string.IsNullOrEmpty(stored) && stored.Length == 60 && stored.StartsWith("$2");
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}

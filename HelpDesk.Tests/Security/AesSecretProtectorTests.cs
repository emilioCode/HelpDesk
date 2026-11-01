using HelpDesk.Infrastructure.Security;
using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace HelpDesk.Tests.Security
{
    public class AesSecretProtectorTests
    {
        private const string Key = "a-test-secret-key-with-more-than-32-chars";
        private readonly AesSecretProtector _protector = new AesSecretProtector(Key);

        [Test]
        public void ProtectThenUnprotect_ReturnsTheOriginalText()
        {
            var stored = _protector.Protect("mail-password");
            Assert.AreNotEqual("mail-password", stored);
            Assert.IsTrue(_protector.IsProtected(stored));
            Assert.AreEqual("mail-password", _protector.Unprotect(stored));
        }

        [Test]
        public void Protect_UsesADifferentIvEachTime()
        {
            Assert.AreNotEqual(_protector.Protect("mail-password"), _protector.Protect("mail-password"));
        }

        [Test]
        public void Protect_DoesNotEncryptTwice()
        {
            var stored = _protector.Protect("mail-password");
            Assert.AreEqual(stored, _protector.Protect(stored));
        }

        [Test]
        public void Protect_StaysWithinTheColumnSizeForTheLongestPassword()
        {
            var stored = _protector.Protect(new string('p', 30));
            Assert.LessOrEqual(stored.Length, 500);
        }

        [Test]
        public void Unprotect_ReturnsALegacyPlainValueUnchanged()
        {
            Assert.AreEqual("old-plain", _protector.Unprotect("old-plain"));
            Assert.IsNull(_protector.Unprotect(null));
        }

        [Test]
        public void Protect_KeepsEmptyValuesEmpty()
        {
            Assert.AreEqual("", _protector.Protect(""));
            Assert.IsNull(_protector.Protect(null));
        }

        [Test]
        public void Unprotect_RejectsAnAlteredValue()
        {
            var stored = _protector.Protect("mail-password");
            var altered = stored.Substring(0, stored.Length - 4) + (stored.EndsWith("AAAA") ? "BBBB" : "AAAA");
            Assert.Throws<CryptographicException>(() => _protector.Unprotect(altered));
        }

        [Test]
        public void Unprotect_RejectsAValueProtectedWithAnotherKey()
        {
            var other = new AesSecretProtector("another-secret-key-with-more-than-32-chars");
            var stored = other.Protect("mail-password");
            Assert.Throws<CryptographicException>(() => _protector.Unprotect(stored));
        }

        [Test]
        public void Constructor_RequiresALongKey()
        {
            Assert.Throws<ArgumentException>(() => new AesSecretProtector("short"));
            Assert.Throws<ArgumentException>(() => new AesSecretProtector(null));
        }
    }
}

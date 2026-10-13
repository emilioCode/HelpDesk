using HelpDesk.Infrastructure.Security;
using NUnit.Framework;

namespace HelpDesk.Tests.Security
{
    public class BCryptPasswordHasherTests
    {
        private readonly BCryptPasswordHasher _hasher = new BCryptPasswordHasher();

        [Test]
        public void Hash_ProducesAHashThatIsNotThePassword()
        {
            var hash = _hasher.Hash("secret1");
            Assert.AreNotEqual("secret1", hash);
            Assert.IsTrue(_hasher.IsHashed(hash));
        }

        [Test]
        public void Hash_OfTheSamePasswordDiffersEachTime()
        {
            Assert.AreNotEqual(_hasher.Hash("secret1"), _hasher.Hash("secret1"));
        }

        [Test]
        public void Verify_AcceptsTheRightPasswordAndRejectsAWrongOne()
        {
            var hash = _hasher.Hash("secret1");
            Assert.IsTrue(_hasher.Verify("secret1", hash));
            Assert.IsFalse(_hasher.Verify("secret2", hash));
        }

        [Test]
        public void Verify_AcceptsALegacyPlainTextValue()
        {
            Assert.IsTrue(_hasher.Verify("root", "root"));
            Assert.IsFalse(_hasher.Verify("other", "root"));
        }

        [Test]
        public void Verify_RejectsEmptyValues()
        {
            Assert.IsFalse(_hasher.Verify("", "root"));
            Assert.IsFalse(_hasher.Verify("root", ""));
            Assert.IsFalse(_hasher.Verify(null, null));
        }

        [TestCase("root", false)]
        [TestCase("$2a$11$short", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsHashed_RecognizesOnlyRealHashes(string stored, bool expected)
        {
            Assert.AreEqual(expected, _hasher.IsHashed(stored));
        }

        [Test]
        public void Verify_DoesNotTreatATextThatLooksLikeAHashAsPlainText()
        {
            var fake = "$2a$11$" + new string('x', 53);
            Assert.IsFalse(_hasher.Verify(fake, fake));
        }
    }
}

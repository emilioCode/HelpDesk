using HelpDesk.Infrastructure.Security;
using NUnit.Framework;
using System;

namespace HelpDesk.Tests.Security
{
    public class InMemoryLoginAttemptTrackerTests
    {
        private DateTime _now;
        private InMemoryLoginAttemptTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _now = new DateTime(2026, 10, 12, 8, 0, 0, DateTimeKind.Utc);
            _tracker = new InMemoryLoginAttemptTracker(3, 15, () => _now);
        }

        [Test]
        public void IsLocked_FalseBeforeTheLimit()
        {
            _tracker.RecordFailure("ana");
            _tracker.RecordFailure("ana");
            Assert.IsFalse(_tracker.IsLocked("ana"));
        }

        [Test]
        public void IsLocked_TrueAtTheLimit()
        {
            for (var i = 0; i < 3; i++) _tracker.RecordFailure("ana");
            Assert.IsTrue(_tracker.IsLocked("ana"));
        }

        [Test]
        public void IsLocked_IsPerAccountAndIgnoresCase()
        {
            for (var i = 0; i < 3; i++) _tracker.RecordFailure("Ana");
            Assert.IsTrue(_tracker.IsLocked("ana"));
            Assert.IsFalse(_tracker.IsLocked("luis"));
        }

        [Test]
        public void IsLocked_ExpiresAfterTheWindow()
        {
            for (var i = 0; i < 3; i++) _tracker.RecordFailure("ana");
            _now = _now.AddMinutes(16);
            Assert.IsFalse(_tracker.IsLocked("ana"));
        }

        [Test]
        public void Reset_ClearsTheFailures()
        {
            for (var i = 0; i < 3; i++) _tracker.RecordFailure("ana");
            _tracker.Reset("ana");
            Assert.IsFalse(_tracker.IsLocked("ana"));
        }

        [Test]
        public void RecordFailure_StartsANewWindowAfterTheOldOneEnded()
        {
            _tracker.RecordFailure("ana");
            _tracker.RecordFailure("ana");
            _now = _now.AddMinutes(20);
            _tracker.RecordFailure("ana");
            Assert.IsFalse(_tracker.IsLocked("ana"));
        }

        [Test]
        public void EmptyAccounts_AreNeverLocked()
        {
            _tracker.RecordFailure("");
            _tracker.RecordFailure(null);
            Assert.IsFalse(_tracker.IsLocked(""));
            Assert.IsFalse(_tracker.IsLocked(null));
        }
    }
}

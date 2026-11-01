using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Concurrent;

namespace HelpDesk.Infrastructure.Security
{
    // Locks an account for a while after repeated failures. State is kept in memory, so it resets on restart
    // and is per instance; a shared store is needed if the app runs on several instances.
    public class InMemoryLoginAttemptTracker : ILoginAttemptTracker
    {
        private readonly int _maxFailures;
        private readonly TimeSpan _window;
        private readonly Func<DateTime> _now;
        private readonly ConcurrentDictionary<string, Entry> _entries = new ConcurrentDictionary<string, Entry>();

        private class Entry
        {
            public int Failures;
            public DateTime First;
        }

        public InMemoryLoginAttemptTracker(int maxFailures = 5, int windowMinutes = 15, Func<DateTime> now = null)
        {
            _maxFailures = maxFailures;
            _window = TimeSpan.FromMinutes(windowMinutes);
            _now = now ?? (() => DateTime.UtcNow);
        }

        public bool IsLocked(string account)
        {
            if (string.IsNullOrEmpty(account) || !_entries.TryGetValue(Key(account), out var entry))
            {
                return false;
            }
            lock (entry)
            {
                if (_now() - entry.First > _window)
                {
                    _entries.TryRemove(Key(account), out _);
                    return false;
                }
                return entry.Failures >= _maxFailures;
            }
        }

        public void RecordFailure(string account)
        {
            if (string.IsNullOrEmpty(account)) return;
            var entry = _entries.GetOrAdd(Key(account), _ => new Entry { First = _now() });
            lock (entry)
            {
                if (_now() - entry.First > _window)
                {
                    entry.First = _now();
                    entry.Failures = 0;
                }
                entry.Failures++;
            }
        }

        public void Reset(string account)
        {
            if (string.IsNullOrEmpty(account)) return;
            _entries.TryRemove(Key(account), out _);
        }

        private static string Key(string account)
        {
            return account.Trim().ToLowerInvariant();
        }
    }
}

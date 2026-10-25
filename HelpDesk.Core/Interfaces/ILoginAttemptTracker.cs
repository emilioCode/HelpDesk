namespace HelpDesk.Core.Interfaces
{
    public interface ILoginAttemptTracker
    {
        bool IsLocked(string account);
        void RecordFailure(string account);
        void Reset(string account);
    }
}

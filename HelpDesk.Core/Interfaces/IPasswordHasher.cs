namespace HelpDesk.Core.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);

        // Accepts a hash, and also a legacy plain-text value so that existing users can sign in once and be migrated.
        bool Verify(string password, string stored);

        bool IsHashed(string stored);
    }
}

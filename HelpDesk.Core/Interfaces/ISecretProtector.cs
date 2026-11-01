namespace HelpDesk.Core.Interfaces
{
    // Reversible encryption for secrets the application must read back, such as the company's mail password.
    public interface ISecretProtector
    {
        string Protect(string plainText);

        // Returns the value unchanged when it was stored before encryption existed.
        string Unprotect(string stored);

        bool IsProtected(string stored);
    }
}

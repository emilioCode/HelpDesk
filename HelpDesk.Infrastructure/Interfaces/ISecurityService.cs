namespace HelpDesk.Infrastructure.Interfaces
{
    public interface ISecurityService
    {
        string Encripting(string password);
        string Decrypting(string password);
    }
}

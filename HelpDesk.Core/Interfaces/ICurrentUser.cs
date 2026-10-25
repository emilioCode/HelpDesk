namespace HelpDesk.Core.Interfaces
{
    // The signed-in user, read from the access token. When there is no token, IsAuthenticated is false
    // and the other values are zero or empty.
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        int Id { get; }
        int CompanyId { get; }
        string Role { get; }
        int Level { get; }
    }
}

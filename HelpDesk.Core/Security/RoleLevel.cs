using HelpDesk.Core.Dictionaries;

namespace HelpDesk.Core.Security
{
    public static class RoleLevel
    {
        public const int Technician = 1;
        public const int Moderator = 2;
        public const int Admin = 3;
        public const int Root = 4;

        // Zero when the role is unknown.
        public static int Of(string role)
        {
            return role != null && Levels.AccessLevel.TryGetValue(role, out var level) ? level : 0;
        }
    }
}

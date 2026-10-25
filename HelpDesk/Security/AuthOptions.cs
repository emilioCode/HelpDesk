namespace HelpDesk.Security
{
    public class AuthOptions
    {
        public const int MinimumKeyLength = 32;

        public string Key { get; set; }
        public string Issuer { get; set; } = "HelpDesk";
        public string Audience { get; set; } = "HelpDesk";
        public const int DefaultExpiresMinutes = 60;

        public int ExpiresMinutes { get; set; } = DefaultExpiresMinutes;

        // When false the API still issues and validates tokens but does not require them (transition mode).
        // It is on unless Auth:Enforce is explicitly set to false.
        public bool Enforce { get; set; } = true;

        // A valid token is also checked against the database, so a user who was disabled stops working before the
        // token expires. The answer is remembered this many seconds per user (0 checks on every request).
        public const int DefaultActiveCheckSeconds = 30;

        public int ActiveCheckSeconds { get; set; } = DefaultActiveCheckSeconds;

        public const string ReportCookie = "hd_token";
    }
}

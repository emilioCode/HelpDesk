namespace HelpDesk.Security
{
    public class AuthOptions
    {
        public const int MinimumKeyLength = 32;

        public string Key { get; set; }
        public string Issuer { get; set; } = "HelpDesk";
        public string Audience { get; set; } = "HelpDesk";
        public int ExpiresMinutes { get; set; } = 480;

        // Transition switch: when false the API still issues and validates tokens but does not require them,
        // so the front end can be updated separately.
        public bool Enforce { get; set; }

        public const string ReportCookie = "hd_token";
    }
}

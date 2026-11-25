namespace HelpDesk.Core.Services
{
    // Settings of the customer tracking page (section "Tracking" of the configuration).
    public class TrackingOptions
    {
        // How long a link keeps working after the order is approved.
        public int DaysAfterApproval { get; set; } = 30;

        // The address customers open (for example https://soporte.example.com). Empty: the address of the request is used.
        public string PublicBaseUrl { get; set; }

        // Calls to the public endpoint allowed per minute from one address.
        public int RequestsPerMinute { get; set; } = 30;
    }
}

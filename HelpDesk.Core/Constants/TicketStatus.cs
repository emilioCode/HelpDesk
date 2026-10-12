namespace HelpDesk.Core.Constants
{
    public static class TicketStatus
    {
        public const string Open = "Abierto";
        public const string InProgress = "en Proceso";
        public const string Completed = "Completado";

        // Uppercase forms, for comparisons made after ToUpper() and for switch labels.
        public static class Upper
        {
            public const string Open = "ABIERTO";
            public const string InProgress = "EN PROCESO";
            public const string Completed = "COMPLETADO";
        }
    }
}

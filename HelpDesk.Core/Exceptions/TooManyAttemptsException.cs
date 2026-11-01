using System;

namespace HelpDesk.Core.Exceptions
{
    // Too many failed sign-ins for the account (HTTP 429).
    public class TooManyAttemptsException : Exception
    {
        public TooManyAttemptsException() : base("Demasiados intentos fallidos. Intente de nuevo en unos minutos.") { }
    }
}

using System;

namespace HelpDesk.Core.Exceptions
{
    // The signed-in user is not allowed to do this (HTTP 403).
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException() : base("No tiene permisos para realizar esta operación.") { }
        public AccessDeniedException(string message) : base(message) { }
    }
}

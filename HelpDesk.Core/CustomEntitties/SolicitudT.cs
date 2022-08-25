using HelpDesk.Core.Entities;

namespace HelpDesk.Core.CustomEntities
{
    public class SolicitudT : Solicitud
    {
        public string claves { get; set; }
        public string cliente { get; set; }
    }
}

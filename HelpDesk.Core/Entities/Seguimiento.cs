using System;

namespace HelpDesk.Core.Entities
{
    public partial class Seguimiento : BaseEntity
    {
        public int IdEmpresa { get; set; }
        public int IdSolicitud { get; set; }
        public int IdUsuario { get; set; }
        public string Texto { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public bool? Favorito { get; set; }
        public bool? Etiquetado { get; set; }
    }
}

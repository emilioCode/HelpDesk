using System;

namespace HelpDesk.Core.DTOs
{
    public class SolicitudLiteDto
    {
        public string TipoSolicitud { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaTermino { get; set; }
        public int IdEmpresa { get; set; }
    }
}

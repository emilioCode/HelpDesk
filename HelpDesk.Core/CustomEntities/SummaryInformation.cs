using System;

namespace HelpDesk.Core.CustomEntities
{
    public class SummaryInformation
    {
        public int Id { get; set; }
        public string NoSecuencia { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public DateTime? FechaTermino { get; set; }
        public TimeSpan? HoraTermino { get; set; }
        public string AtendidoPor { get; set; }
        public string Cliente { get; set; }
        public string TipoSolicitud { get; set; }
        public string TipoServicio { get; set; }
        public string Estado { get; set; }
        public string AprobadoPor { get; set; }
        public int IdEmpresa { get; set; }
    }
}

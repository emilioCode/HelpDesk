using System;
using System.Collections.Generic;

namespace HelpDesk.Models
{
    public partial class Solicitud
    {
        public int Id { get; set; }
        public string NoSecuencia { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public DateTime? FechaTermino { get; set; }
        public TimeSpan? HoraTermino { get; set; }
        public int IdUsuario { get; set; }
        public int IdCliente { get; set; }
        public string TipoSolicitud { get; set; }
        public string TipoServicio { get; set; }
        public string Estado { get; set; }
        public int IdEmpresa { get; set; }
        public string Descripcion { get; set; }
        public int? AprobadoPor { get; set; }
        public bool? Habilitado { get; set; }
    }
}

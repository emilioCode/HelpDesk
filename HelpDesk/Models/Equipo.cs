using System;
using System.Collections.Generic;

namespace HelpDesk.Models
{
    public partial class Equipo
    {
        public int Id { get; set; }
        public int IdSolicitud { get; set; }
        public int IdEmpresa { get; set; }
        public string Descripcion { get; set; }
        public string FallaReportada { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string NoSerial { get; set; }
        public bool? Habilitado { get; set; }
    }
}

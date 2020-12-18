using System;
using System.Collections.Generic;

namespace HelpDesk.Models
{
    public partial class Piezas
    {
        public int Id { get; set; }
        public int IdSolicitud { get; set; }
        public int IdEmpresa { get; set; }
        public int Cantidad { get; set; }
        public string NoSerial { get; set; }
        public string Descripcion { get; set; }
        public bool? Habilitado { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace HelpDesk.Models
{
    public partial class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string Extension { get; set; }
        public string TipoCliente { get; set; }
        public string Correo { get; set; }
        public string Departamento { get; set; }
        public string Direccion { get; set; }
        public int IdEmpresa { get; set; }
        public bool? Habilitado { get; set; }
    }
}

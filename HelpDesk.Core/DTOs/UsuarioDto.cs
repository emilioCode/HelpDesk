﻿namespace HelpDesk.Core.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NumDocumento { get; set; }
        public string CuentaUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Acceso { get; set; }
        public string Correo { get; set; }
        public int? IdEmpresa { get; set; }
        public byte[] Image { get; set; }
        public bool? Habilitado { get; set; }
    }
}

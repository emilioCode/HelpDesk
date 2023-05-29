namespace HelpDesk.Core.Entities
{
    public partial class Usuario : BaseEntity
    {
        public string Nombre { get; set; }
        public string NumDocumento { get; set; }
        public string CuentaUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Acceso { get; set; }
        public string Correo { get; set; }
        public int IdEmpresa { get; set; }
        public byte[] Image { get; set; }
    }
}

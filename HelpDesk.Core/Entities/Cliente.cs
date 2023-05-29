namespace HelpDesk.Core.Entities
{
    public partial class Cliente : BaseEntity
    {
        public string Nombre { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string Extension { get; set; }
        public string Rnc { get; set; }
        public string Correo { get; set; }
        public string Departamento { get; set; }
        public string Direccion { get; set; }
        public int IdEmpresa { get; set; }
    }
}

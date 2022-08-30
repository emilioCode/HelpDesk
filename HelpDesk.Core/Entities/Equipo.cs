namespace HelpDesk.Core.Entities
{
    public partial class Equipo : BaseEntity
    {
        public int IdSolicitud { get; set; }
        public int IdEmpresa { get; set; }
        public string Descripcion { get; set; }
        public string FallaReportada { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string NoSerial { get; set; }
    }
}

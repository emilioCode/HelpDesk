namespace HelpDesk.Core.Entities
{
    public partial class Piezas : BaseEntity
    {
        public int IdSolicitud { get; set; }
        public int IdEmpresa { get; set; }
        public int Cantidad { get; set; }
        public string NoSerial { get; set; }
        public string Descripcion { get; set; }
    }
}

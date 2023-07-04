namespace HelpDesk.Core.DTOs
{
    public class PiezasDto
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

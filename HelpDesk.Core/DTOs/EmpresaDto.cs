namespace HelpDesk.Core.DTOs
{
    public class EmpresaDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; }
        public string SectorComercial { get; set; }
        public string Rnc { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public string Url { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Direccion { get; set; }
        public byte[] Image { get; set; }
        public string NoAutorizacion { get; set; }
        public string Secuenciaticket { get; set; }
        public int? Limit { get; set; }
        public string CondicionesTaller { get; set; }
        public string CondicionesDomicilio { get; set; }
        public bool? Habilitado { get; set; }
    }
}

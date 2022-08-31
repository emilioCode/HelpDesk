namespace HelpDesk.Core.CustomEntitties
{
    public class UserIdentity
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Acceso { get; set; }
        public int IdEmpresa { get; set; }
        public string Empresa { get; set; }
        public string UserName { get; set; }
        public byte[] Image { get; set; }
        public byte[] Logo { get; set; }
    }
}

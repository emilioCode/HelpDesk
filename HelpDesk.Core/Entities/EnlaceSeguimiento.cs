using System;

namespace HelpDesk.Core.Entities
{
    // The link a customer uses to follow an order without signing in. The token is random and the only thing the link carries.
    public class EnlaceSeguimiento : BaseEntity, ICompanyEntity
    {
        public int IdEmpresa { get; set; }
        public int IdSolicitud { get; set; }
        public string Token { get; set; }
        public DateTime CreadoEn { get; set; }
        // After this moment the link stops working. Empty while the order is not approved.
        public DateTime? VenceEn { get; set; }
        public DateTime? RevocadoEn { get; set; }
    }
}

namespace HelpDesk.Core.Entities
{
    // These entities belong to one company and are checked against the signed-in user's company.
    public partial class Solicitud : ICompanyEntity { }
    public partial class Equipo : ICompanyEntity { }
    public partial class Piezas : ICompanyEntity { }
    public partial class Seguimiento : ICompanyEntity { }
    public partial class Cliente : ICompanyEntity { }
    public partial class Usuario : ICompanyEntity { }
}

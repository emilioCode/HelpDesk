using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using System.Collections.Generic;

namespace HelpDesk.Core.Interfaces
{
    // The conversions between the entities of the database and the DTOs of the API. Only the ones the controllers use.
    // The answers of every one of them (also for nothing at all, and for lists) are written down in HelpDesk.Tests/Mappings/Golden.
    public interface IHelpDeskMapper
    {
        EquipoDto Map(Equipo source);
        Equipo Map(EquipoDto source);
        List<Equipo> Map(List<EquipoDto> source);
        IEnumerable<EquipoDto> Map(IEnumerable<Equipo> source);

        UsuarioDto Map(Usuario source);
        Usuario Map(UsuarioDto source);
        IEnumerable<UsuarioDto> Map(IEnumerable<Usuario> source);

        SeguimientoDto Map(Seguimiento source);
        Seguimiento Map(SeguimientoDto source);

        Cliente Map(ClienteDto source);
        IEnumerable<ClienteDto> Map(IEnumerable<Cliente> source);

        EmpresaDto Map(Empresa source);
        Empresa Map(EmpresaDto source);
        IEnumerable<EmpresaDto> Map(IEnumerable<Empresa> source);

        Solicitud Map(SolicitudDto source);
        Solicitud Map(SolicitudLiteDto source);

        PiezasDto Map(Piezas source);
        Piezas Map(PiezasDto source);
        IEnumerable<PiezasDto> Map(IEnumerable<Piezas> source);
    }
}

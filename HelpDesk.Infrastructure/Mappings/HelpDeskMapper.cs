using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Infrastructure.Mappings
{
    // The conversions between entities and DTOs, written at build time by Mapperly (the code it generates is plain C#: no reflection and no
    // library at run time). A property that is not mapped is a build error, not a surprise in production.
    // It answers exactly what the mapper it replaced (AutoMapper) answered, which HelpDesk.Tests/Mappings/Golden records. Its habits that
    // are kept on purpose:
    //  - nothing in gives nothing out, but no list at all gives an empty list;
    //  - a user without a company number (null) becomes company 0, and company 0 goes out as no company (null);
    //  - a user DTO without a photo becomes a user with an empty photo (not with no photo); everywhere else the absence of a photo is kept.
    [Mapper]
    public partial class HelpDeskMapper : IHelpDeskMapper
    {
        // ---- devices
        public partial EquipoDto Map(Equipo source);
        public partial Equipo Map(EquipoDto source);
        public List<Equipo> Map(List<EquipoDto> source) => MapList(source, Map);
        public IEnumerable<EquipoDto> Map(IEnumerable<Equipo> source) => MapList(source, Map);

        // ---- users
        [MapProperty(nameof(Usuario.IdEmpresa), nameof(UsuarioDto.IdEmpresa), Use = nameof(ZeroToNothing))]
        [MapProperty(nameof(Usuario.Image), nameof(UsuarioDto.Image), Use = nameof(PhotoAsIs))]
        public partial UsuarioDto Map(Usuario source);

        [MapProperty(nameof(UsuarioDto.IdEmpresa), nameof(Usuario.IdEmpresa), Use = nameof(NothingToZero))]
        [MapProperty(nameof(UsuarioDto.Image), nameof(Usuario.Image), Use = nameof(NothingToEmptyPhoto))]
        public partial Usuario Map(UsuarioDto source);

        public IEnumerable<UsuarioDto> Map(IEnumerable<Usuario> source) => MapList(source, Map);

        // ---- activities
        public partial SeguimientoDto Map(Seguimiento source);
        public partial Seguimiento Map(SeguimientoDto source);

        // ---- customers
        public partial Cliente Map(ClienteDto source);
        private partial ClienteDto MapCustomer(Cliente source);
        public IEnumerable<ClienteDto> Map(IEnumerable<Cliente> source) => MapList(source, MapCustomer);

        // ---- companies
        [MapProperty(nameof(Empresa.Image), nameof(EmpresaDto.Image), Use = nameof(PhotoAsIs))]
        public partial EmpresaDto Map(Empresa source);

        [MapProperty(nameof(EmpresaDto.Image), nameof(Empresa.Image), Use = nameof(PhotoAsIs))]
        public partial Empresa Map(EmpresaDto source);
        public IEnumerable<EmpresaDto> Map(IEnumerable<Empresa> source) => MapList(source, Map);

        // ---- orders
        public partial Solicitud Map(SolicitudDto source);

        // The light request only says which kind of order, the dates and the company; nothing else of the order comes with it.
        [MapperIgnoreTarget(nameof(Solicitud.Id))]
        [MapperIgnoreTarget(nameof(Solicitud.Habilitado))]
        [MapperIgnoreTarget(nameof(Solicitud.NoSecuencia))]
        [MapperIgnoreTarget(nameof(Solicitud.FechaCreacion))]
        [MapperIgnoreTarget(nameof(Solicitud.HoraInicio))]
        [MapperIgnoreTarget(nameof(Solicitud.HoraTermino))]
        [MapperIgnoreTarget(nameof(Solicitud.IdUsuario))]
        [MapperIgnoreTarget(nameof(Solicitud.IdCliente))]
        [MapperIgnoreTarget(nameof(Solicitud.TipoServicio))]
        [MapperIgnoreTarget(nameof(Solicitud.Estado))]
        [MapperIgnoreTarget(nameof(Solicitud.Descripcion))]
        [MapperIgnoreTarget(nameof(Solicitud.AprobadoPor))]
        public partial Solicitud Map(SolicitudLiteDto source);

        // ---- parts
        public partial PiezasDto Map(Piezas source);
        public partial Piezas Map(PiezasDto source);
        public IEnumerable<PiezasDto> Map(IEnumerable<Piezas> source) => MapList(source, Map);

        // ---- the habits above
        private static List<TTarget> MapList<TSource, TTarget>(IEnumerable<TSource> source, System.Func<TSource, TTarget> map)
        {
            return source == null ? new List<TTarget>() : source.Select(map).ToList();
        }

        private static int? ZeroToNothing(int companyId) => companyId != 0 ? companyId : (int?)null;

        private static int NothingToZero(int? companyId) => companyId ?? 0;

        private static byte[] NothingToEmptyPhoto(byte[] photo) => photo ?? new byte[0];

        // Mapperly would turn no photo into an empty one; here the absence is kept.
        private static byte[] PhotoAsIs(byte[] photo) => photo;
    }
}

using AutoMapper;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;

namespace HelpDesk.Infrastructure.Mappings
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<UsuarioDto, Usuario>();
            CreateMap<Usuario, UsuarioDto>().AfterMap((src, dest) =>
                {
                    if (src.IdEmpresa != 0)
                    {
                        dest.IdEmpresa = src.IdEmpresa;
                    }
                    else
                    {
                        dest.IdEmpresa = null;
                    }

                    if (src.Image != null)
                    {
                        dest.Image = dest.Image;
                    }
                    else
                    {
                        dest.Image = null;
                    }
                });
            CreateMap<UserLogin, Usuario>();
            CreateMap<EmpresaDto, Empresa>().AfterMap((src, dest) =>
                {
                    if (src.Image != null)
                    {
                        dest.Image = dest.Image;
                    }
                    else
                    {
                        dest.Image = null;
                    }
                });
            CreateMap<Empresa, EmpresaDto>().AfterMap((src, dest) =>
                {
                    if (src.Image != null)
                    {
                        dest.Image = dest.Image;
                    }
                    else
                    {
                        dest.Image = null;
                    }
                });
            CreateMap<ClienteDto, Cliente>();
            CreateMap<Cliente, ClienteDto>();

            CreateMap<Solicitud, SolicitudT>();
            //    .AfterMap(async (src, dest) =>
            //{
            //    var customer = await _unitOfWork.CustomerRepository.GetById(src.IdCliente);
            //    var codes1 = customer.Nombre + ", " + customer.Contacto;
            //    var eq = _unitOfWork.DeviceRepository.GetDevicesByTicketId(src.Id);
            //    var codes2 = "";
            //    eq.ToList().ForEach(r =>
            //    {
            //        codes2 = codes2 + r.NoSerial + ", ";
            //    });
            //    dest.cliente = customer.Nombre;
            //    dest.claves = codes1 + ", " + codes2;
            //});
            CreateMap<SolicitudDto, Solicitud>();
            CreateMap<Solicitud, SolicitudDto>();
            CreateMap<SolicitudLiteDto, Solicitud>();
            CreateMap<Equipo, EquipoDto>();
            CreateMap<EquipoDto, Equipo>();
            CreateMap<SeguimientoDto, Seguimiento>();
            CreateMap<Seguimiento, SeguimientoDto>();
        }
    }
}

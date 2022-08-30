using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using System;

namespace HelpDesk.Infrastructure.Mappings
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<UsuarioDto, Usuario>();
            CreateMap<Usuario, UsuarioDto>()
                .AfterMap((src, dest) =>
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

        }
    }
}

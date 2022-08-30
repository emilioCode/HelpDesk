using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Solicitud>
    {
        public void Configure(EntityTypeBuilder<Solicitud> entity)
        {
            entity.ToTable("SOLICITUD");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.AprobadoPor).HasColumnName("aprobadoPor");

            entity.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .IsUnicode(false);

            entity.Property(e => e.Estado)
                .IsRequired()
                .HasColumnName("estado")
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('Abierto')");

            entity.Property(e => e.FechaCreacion)
                .HasColumnName("fechaCreacion")
                .HasColumnType("date");

            entity.Property(e => e.FechaInicio)
                .HasColumnName("fechaInicio")
                .HasColumnType("date");

            entity.Property(e => e.FechaTermino)
                .HasColumnName("fechaTermino")
                .HasColumnType("date");

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.HoraInicio)
                .HasColumnName("horaInicio")
                .HasColumnType("time(0)");

            entity.Property(e => e.HoraTermino)
                .HasColumnName("horaTermino")
                .HasColumnType("time(0)");

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");

            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.Property(e => e.NoSecuencia)
                .IsRequired()
                .HasColumnName("noSecuencia")
                .HasMaxLength(12)
                .IsUnicode(false);

            entity.Property(e => e.TipoServicio)
                .IsRequired()
                .HasColumnName("tipoServicio")
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.TipoSolicitud)
                .IsRequired()
                .HasColumnName("tipoSolicitud")
                .HasMaxLength(20)
                .IsUnicode(false);
        }
    }
}

using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class TraceConfiguration : IEntityTypeConfiguration<Seguimiento>
    {
        public void Configure(EntityTypeBuilder<Seguimiento> entity)
        {
            entity.ToTable("SEGUIMIENTO");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Etiquetado).HasColumnName("etiquetado");

            entity.Property(e => e.Favorito).HasColumnName("favorito");

            entity.Property(e => e.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("date");

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.Hora)
                .HasColumnName("hora")
                .HasColumnType("time(0)");

            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");

            entity.Property(e => e.IdSolicitud).HasColumnName("idSolicitud");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.Property(e => e.Texto)
                .IsRequired()
                .HasColumnName("texto")
                .IsUnicode(false);
        }
    }
}

using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class ComponentConfiguration : IEntityTypeConfiguration<Equipo>
    {
        public void Configure(EntityTypeBuilder<Equipo> entity)
        {
            entity.ToTable("EQUIPO");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .IsUnicode(false);

            entity.Property(e => e.FallaReportada)
                .IsRequired()
                .HasColumnName("fallaReportada")
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");

            entity.Property(e => e.IdSolicitud).HasColumnName("idSolicitud");

            entity.Property(e => e.Marca)
                .IsRequired()
                .HasColumnName("marca")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Modelo)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.NoSerial)
                .IsRequired()
                .HasColumnName("noSerial")
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}

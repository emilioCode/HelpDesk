using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class TrackingConfiguration : IEntityTypeConfiguration<EnlaceSeguimiento>
    {
        public void Configure(EntityTypeBuilder<EnlaceSeguimiento> entity)
        {
            entity.ToTable("ENLACE_SEGUIMIENTO");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");
            entity.Property(e => e.IdSolicitud).HasColumnName("idSolicitud");
            entity.Property(e => e.Token).IsRequired().HasColumnName("token").HasMaxLength(64).IsUnicode(false);
            entity.Property(e => e.CreadoEn).HasColumnName("creadoEn").HasColumnType("datetime2");
            entity.Property(e => e.VenceEn).HasColumnName("venceEn").HasColumnType("datetime2");
            entity.Property(e => e.RevocadoEn).HasColumnName("revocadoEn").HasColumnType("datetime2");
            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.IdSolicitud);
        }
    }
}

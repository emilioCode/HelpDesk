using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class BusinessConfiguration : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> entity)
        {
            entity.ToTable("EMPRESA");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CondicionesDomicilio)
                .HasColumnName("condicionesDomicilio")
                .IsUnicode(false);

            entity.Property(e => e.CondicionesTaller)
                .HasColumnName("condicionesTaller")
                .IsUnicode(false);

            entity.Property(e => e.Contrasena)
                .HasColumnName("contrasena")
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasColumnName("correo")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Direccion)
                .HasColumnName("direccion")
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.Host)
                .HasColumnName("host")
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.Image)
                .HasColumnName("image")
                .HasColumnType("image");

            entity.Property(e => e.Limit)
                .HasColumnName("limit")
                .HasDefaultValueSql("((1))");

            entity.Property(e => e.NoAutorizacion)
                .HasColumnName("noAutorizacion")
                .HasMaxLength(12)
                .IsUnicode(false);

            entity.Property(e => e.Port).HasColumnName("port");

            entity.Property(e => e.RazonSocial)
                .IsRequired()
                .HasColumnName("razon_social")
                .IsUnicode(false);

            entity.Property(e => e.Rnc)
                .HasColumnName("rnc")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.SectorComercial)
                .HasColumnName("sector_comercial")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Secuenciaticket)
                .IsRequired()
                .HasColumnName("secuenciaticket")
                .HasMaxLength(12)
                .IsUnicode(false);

            entity.Property(e => e.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Url)
                .HasColumnName("url")
                .HasMaxLength(80)
                .IsUnicode(false);
        }
    }
}

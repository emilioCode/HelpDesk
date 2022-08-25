using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> entity)
        {
            entity.ToTable("CLIENTE");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Contacto)
                .HasColumnName("contacto")
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasColumnName("correo")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Departamento)
                .HasColumnName("departamento")
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.Direccion)
                .HasColumnName("direccion")
                .IsUnicode(false);

            entity.Property(e => e.Extension)
                .HasColumnName("extension")
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Rnc)
                .HasColumnName("rnc")
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(20)
                .IsUnicode(false);
        }
    }
}

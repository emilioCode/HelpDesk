using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> entity)
        {
            entity.ToTable("USUARIO");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Acceso)
                .HasColumnName("acceso")
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Contrasena)
                .IsRequired()
                .HasColumnName("contrasena")
                .IsUnicode(false);

            entity.Property(e => e.Correo)
                .HasColumnName("correo")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.CuentaUsuario)
                .IsRequired()
                .HasColumnName("cuenta_usuario")
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");

            entity.Property(e => e.Image)
                .HasColumnName("image")
                .HasColumnType("image");

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasColumnName("nombre")
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.NumDocumento)
                .HasColumnName("num_documento")
                .HasMaxLength(30)
                .IsUnicode(false);
        }
    }
}

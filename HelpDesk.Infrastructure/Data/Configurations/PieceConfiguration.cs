using HelpDesk.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Data.Configurations
{
    public class PieceConfiguration : IEntityTypeConfiguration<Piezas>
    {
        public void Configure(EntityTypeBuilder<Piezas> entity)
        {
            entity.ToTable("PIEZAS");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Cantidad).HasColumnName("cantidad");

            entity.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .IsUnicode(false);

            entity.Property(e => e.Habilitado).HasColumnName("habilitado");

            entity.Property(e => e.IdEmpresa).HasColumnName("idEmpresa");

            entity.Property(e => e.IdSolicitud).HasColumnName("idSolicitud");

            entity.Property(e => e.NoSerial)
                .IsRequired()
                .HasColumnName("noSerial")
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}

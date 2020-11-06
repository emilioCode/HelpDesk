using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelpDesk.Models
{
    public partial class HelpDeskDBContext : DbContext
    {
        public HelpDeskDBContext()
        {
        }

        public HelpDeskDBContext(DbContextOptions<HelpDeskDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cliente> Cliente { get; set; }
        public virtual DbSet<Empresa> Empresa { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
//                optionsBuilder.UseSqlServer("Server=ASUS;Database=HelpDeskDB;User Id=sa;Password=123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("CLIENTE");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Contacto)
                    .HasColumnName("contacto")
                    .HasMaxLength(80)
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

                entity.Property(e => e.Telefono)
                    .HasColumnName("telefono")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TipoCliente)
                    .HasColumnName("tipoCliente")
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("EMPRESA");

                entity.Property(e => e.Id).HasColumnName("id");

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
                    .HasMaxLength(20)
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
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasMaxLength(80)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Usuario>(entity =>
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
            });
        }
    }
}

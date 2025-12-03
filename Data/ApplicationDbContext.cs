using Microsoft.EntityFrameworkCore;
using test_api.Models;

namespace test_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                entity.Property(e => e.Activa)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Nombre).IsUnique();

                entity.HasMany(e => e.Productos)
                    .WithOne(p => p.Categoria)
                    .HasForeignKey(p => p.CategoriaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                entity.Property(e => e.Precio)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(e => e.Stock)
                    .IsRequired();

                entity.Property(e => e.Activo)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.CategoriaId)
                    .IsRequired();

                entity.HasIndex(e => e.Nombre).IsUnique();

                entity.HasOne(e => e.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

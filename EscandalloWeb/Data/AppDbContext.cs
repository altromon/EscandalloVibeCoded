using EscandalloWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EscandalloWeb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RegistroPago> Pagos => Set<RegistroPago>();
    public DbSet<CentroEstetica> Centros => Set<CentroEstetica>();
    public DbSet<GastoGeneral> GastosGenerales => Set<GastoGeneral>();
    public DbSet<CategoriaPersonal> Personal => Set<CategoriaPersonal>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<ServicioProducto> ServicioProductos => Set<ServicioProducto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<CentroEstetica>()
            .HasIndex(c => c.UsuarioId);

        modelBuilder.Entity<Servicio>()
            .HasOne(s => s.CategoriaPersonal)
            .WithMany()
            .HasForeignKey(s => s.CategoriaPersonalId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ServicioProducto>()
            .HasOne(sp => sp.Producto)
            .WithMany()
            .HasForeignKey(sp => sp.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetConnect.Models;

namespace PetConnect.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Noticia> Noticias { get; set; }
    public DbSet<Comentario> Comentarios { get; set; }

    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<AdopcionDetalle> AdopcionDetalles { get; set; }

    public DbSet<VeterinariaDetalle> VeterinariaDetalles { get; set; }
    public DbSet<Resena> Resenas { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Servicio>()
            .HasOne(s => s.AdopcionDetalle)
            .WithOne(ad => ad.Servicio)
            .HasForeignKey<AdopcionDetalle>(ad => ad.ServicioId);

        modelBuilder.Entity<Servicio>()
            .HasOne(s => s.VeterinariaDetalle)
            .WithOne(vd => vd.Servicio)
            .HasForeignKey<VeterinariaDetalle>(vd => vd.ServicioId);
        
        modelBuilder.Entity<VeterinariaDetalle>()
            .HasMany(vd => vd.Resenas)
            .WithOne(r => r.VeterinariaDetalle)
            .HasForeignKey(r => r.VeterinariaDetalleId);

    }
}

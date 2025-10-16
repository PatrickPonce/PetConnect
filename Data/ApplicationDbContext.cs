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
    
    public DbSet<PetShopDetalle> PetShopDetalles { get; set; }
    public DbSet<Favorito> Favoritos { get; set; }
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

        modelBuilder.Entity<Servicio>()
            .HasOne(s => s.PetShopDetalle)
            .WithOne(psd => psd.Servicio)
            .HasForeignKey<PetShopDetalle>(psd => psd.ServicioId);

        modelBuilder.Entity<Favorito>()
            .HasKey(f => new { f.UsuarioId, f.NoticiaId });


        modelBuilder.Entity<Favorito>()
            .HasOne(f => f.Usuario)
            .WithMany() 
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Favorito>()
            .HasOne(f => f.Noticia)
            .WithMany() 
            .HasForeignKey(f => f.NoticiaId);

    }   
}

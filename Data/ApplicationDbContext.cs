using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetConnect.Models;
using Microsoft.AspNetCore.Identity;

namespace PetConnect.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
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
    public DbSet<ConfiguracionSitio> ConfiguracionesSitio { get; set; }
 


    public DbSet<LugarPetFriendly> LugaresPetFriendly { get; set; }
    public DbSet<ComentarioLugar> ComentariosLugar { get; set; }
    public DbSet<FavoritoLugar> FavoritosLugar { get; set; }
 
 
 
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
    }   

}

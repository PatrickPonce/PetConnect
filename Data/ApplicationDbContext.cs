using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetConnect.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace PetConnect.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>, IDataProtectionKeyContext
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
        public DbSet<Guarderia> Guarderias { get; set; }
        public DbSet<ComentarioGuarderia> ComentariosGuarderia { get; set; }
        public DbSet<FavoritoGuarderia> FavoritosGuarderia { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public object Configuration { get; internal set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

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
            
            
            // Configuración para la clave compuesta de FavoritoLugar
            modelBuilder.Entity<FavoritoLugar>()
                .HasKey(f => new { f.LugarPetFriendlyId, f.UsuarioId });

            // Configuración para la clave compuesta de FavoritoGuarderia
            modelBuilder.Entity<FavoritoGuarderia>()
                .HasKey(f => new { f.GuarderiaId, f.UsuarioId });
            
            modelBuilder.Entity<Favorito>()
            .HasKey(f => new { f.UsuarioId, f.NoticiaId });
            modelBuilder.Entity<Favorito>(entity =>
            {
                entity.HasKey(f => new { f.UsuarioId, f.NoticiaId });
                entity.HasOne(f => f.Usuario)
                    .WithMany() 
                    .HasForeignKey(f => f.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(f => f.Noticia)
                    .WithMany(n => n.Favoritos)
                    .HasForeignKey(f => f.NoticiaId);
            });
           
        }   
    }
}
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
        // ... (después de DbSet<FavoritoGuarderia>)
        public DbSet<ProductoPetShop> ProductosPetShop { get; set; }
        public DbSet<FavoritoProducto> FavoritosProducto { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }

        public DbSet<ComentarioServicio> ComentariosServicio { get; set; }
        public DbSet<FavoritoServicio> FavoritosServicio { get; set; }
        public object Configuration { get; internal set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<ResenaProducto> ResenasProducto { get; set; }

        public DbSet<Faq> Faqs { get; set; }
        public DbSet<LugarPetFriendlyDetalle> LugarPetFriendlyDetalles { get; set; }
        public DbSet<GuarderiaDetalle> GuarderiaDetalles { get; set; }

        public DbSet<Pago> Pagos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CONFIGURACIONES DE RELACIONES (YA LAS TENÍAS) ---
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
            
            modelBuilder.Entity<Servicio>()
                .HasOne(s => s.LugarPetFriendlyDetalle)
                .WithOne(l => l.Servicio)
                .HasForeignKey<LugarPetFriendlyDetalle>(l => l.ServicioId);

            modelBuilder.Entity<Servicio>()
                .HasOne(s => s.GuarderiaDetalle)
                .WithOne(g => g.Servicio)
                .HasForeignKey<GuarderiaDetalle>(g => g.ServicioId);

            // --- CONFIGURACIONES DE CLAVES COMPUESTAS (YA LAS TENÍAS) ---
            modelBuilder.Entity<FavoritoLugar>()
                .HasKey(f => new { f.LugarPetFriendlyId, f.UsuarioId });

            modelBuilder.Entity<FavoritoGuarderia>()
                .HasKey(f => new { f.GuarderiaId, f.UsuarioId });
            
            modelBuilder.Entity<FavoritoProducto>()
                .HasKey(f => new { f.ProductoPetShopId, f.UsuarioId });
            
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

            modelBuilder.Entity<FavoritoServicio>()
                .HasKey(f => new { f.ServicioId, f.UsuarioId });

                
            // --- ✅ INICIO DE LA CORRECCIÓN COMPLETA ---
            // Configura TODAS las claves primarias 'Id' (int) para que sean autoincrementables
            
            // Tablas Principales
            modelBuilder.Entity<Noticia>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<Servicio>().Property(e => e.Id).UseIdentityByDefaultColumn(); // <-- ¡ESTA ES LA LÍNEA MÁS IMPORTANTE PARA TU ERROR!
            modelBuilder.Entity<LugarPetFriendly>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<Guarderia>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<ProductoPetShop>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<Faq>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<ConfiguracionSitio>().Property(e => e.Id).UseIdentityByDefaultColumn();

            // Tablas de Detalles
            modelBuilder.Entity<VeterinariaDetalle>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<PetShopDetalle>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<AdopcionDetalle>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<LugarPetFriendlyDetalle>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<GuarderiaDetalle>().Property(e => e.Id).UseIdentityByDefaultColumn();

            // Tablas de Comentarios y Reseñas
            modelBuilder.Entity<Comentario>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<ComentarioLugar>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<ComentarioGuarderia>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<ComentarioServicio>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<Resena>().Property(e => e.Id).UseIdentityByDefaultColumn();
            modelBuilder.Entity<ResenaProducto>().Property(e => e.Id).UseIdentityByDefaultColumn();

            // Tablas de Favoritos (solo si tienen su propia PK 'Id', como FavoritoServicio)
            modelBuilder.Entity<FavoritoServicio>().Property(e => e.Id).UseIdentityByDefaultColumn();

            // --- FIN DE LA CORRECCIÓN ---
        }   
    }
}
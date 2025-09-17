using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetConnect.Models;

namespace PetConnect.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Tablas personalizadas de la aplicación
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Noticia> Noticias { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<Comentario> Comentarios { get; set; }
    public DbSet<Resena> Resenas { get; set; }
    public DbSet<Favorito> Favoritos { get; set; }
    public DbSet<ConfiguracionSitio> ConfiguracionSitio { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Configura las tablas de Identity

        // Mapeo a nombres en minúsculas para PostgreSQL
        modelBuilder.Entity<Categoria>().ToTable("categorias");
        modelBuilder.Entity<Noticia>().ToTable("noticias");
        modelBuilder.Entity<Servicio>().ToTable("servicios");
        modelBuilder.Entity<Comentario>().ToTable("comentarios");
        modelBuilder.Entity<Resena>().ToTable("resenas");
        modelBuilder.Entity<Favorito>().ToTable("favoritos");
        modelBuilder.Entity<ConfiguracionSitio>().ToTable("configuracionsitio");
    }
}
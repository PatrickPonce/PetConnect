// Models/FavoritoNoticia.cs
using Microsoft.AspNetCore.Identity;
using PetConnect.Models;

public class FavoritoNoticia
{
    // Clave foránea para Noticia
    public int NoticiaId { get; set; }
    public virtual Noticia Noticia { get; set; }

    // Clave foránea para el Usuario
    public string UsuarioId { get; set; }
    public virtual IdentityUser Usuario { get; set; }
}
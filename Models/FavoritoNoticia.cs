using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    public class FavoritoNoticia
    {
        public int NoticiaId { get; set; }
        public virtual Noticia Noticia { get; set; }
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    public class FavoritoGuarderia
    {
        public int GuarderiaId { get; set; }
        public virtual Guarderia Guarderia { get; set; }
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
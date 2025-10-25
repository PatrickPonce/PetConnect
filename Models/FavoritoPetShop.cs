using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    public class FavoritoPetShop
    {
        public int PetShopId { get; set; }
        public virtual PetShop PetShop { get; set; }
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
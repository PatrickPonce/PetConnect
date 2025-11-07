// Models/FavoritoProducto.cs
using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    public class FavoritoProducto
    {
        public int ProductoPetShopId { get; set; }
        public virtual ProductoPetShop ProductoPetShop { get; set; }

        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
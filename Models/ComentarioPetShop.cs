using Microsoft.AspNetCore.Identity;
using System;

namespace PetConnect.Models
{
    public class ComentarioPetShop
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public DateTime FechaComentario { get; set; }
        public int PetShopId { get; set; }
        public virtual PetShop PetShop { get; set; }
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
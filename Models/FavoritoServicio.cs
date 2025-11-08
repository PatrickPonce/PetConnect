using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    public class FavoritoServicio
    {
        public int Id { get; set; }

        public int ServicioId { get; set; }
        public virtual Servicio Servicio { get; set; }

        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
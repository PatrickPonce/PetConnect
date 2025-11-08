using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class ComentarioServicio
    {
        public int Id { get; set; }

        [Required]
        public string Texto { get; set; }
        public DateTime FechaComentario { get; set; }

        // Relación con Servicio (la veterinaria)
        public int ServicioId { get; set; }
        public virtual Servicio Servicio { get; set; }

        // Relación con el Usuario que comenta
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
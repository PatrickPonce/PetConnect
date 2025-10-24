using Microsoft.AspNetCore.Identity;
using System;

namespace PetConnect.Models
{
    public class ComentarioGuarderia
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public DateTime FechaComentario { get; set; }
        public int GuarderiaId { get; set; }
        public virtual Guarderia Guarderia { get; set; }
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
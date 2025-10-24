using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // Necesario para la relación con el usuario

namespace PetConnect.Models
{
    public class ComentarioLugar
    {
        public int Id { get; set; }

        [Required]
        public string Texto { get; set; }

        public DateTime FechaComentario { get; set; }

        // Foreign Key para LugarPetFriendly
        public int LugarPetFriendlyId { get; set; }
        public LugarPetFriendly LugarPetFriendly { get; set; }

        // Foreign Key para el usuario que comenta (asumiendo que usas ASP.NET Identity)
        public string UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public IdentityUser Usuario { get; set; }
    }
}
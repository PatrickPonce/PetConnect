using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; 
using System.Collections.Generic;

namespace PetConnect.Models
{
    public class Favorito
    {
        [Required]
        public required string  UsuarioId { get; set; }

        [Required]
        public int NoticiaId { get; set; }

        public IdentityUser Usuario { get; set; } = default!;
        public Noticia Noticia { get; set; } = default!;

        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
    }
}
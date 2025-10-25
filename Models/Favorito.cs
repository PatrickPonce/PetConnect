
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; 

namespace PetConnect.Models
{
    public class Favorito
    {
        [Required]
        public string UsuarioId { get; set; }
        
        // 2. Clave Foránea a la Noticia
        [Required]
        public int NoticiaId { get; set; }

        public IdentityUser Usuario { get; set; } = default!;
        public Noticia Noticia { get; set; } = default!;

        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    public class ResenaProducto
    {
        public int Id { get; set; }
        
        [Range(1, 5)]
        public int Puntuacion { get; set; } // De 1 a 5 estrellas
        
        [Required(ErrorMessage = "El comentario no puede estar vacío.")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        public string Texto { get; set; } = string.Empty;
        
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Relaciones
        public string UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual IdentityUser Usuario { get; set; }

        public int ProductoPetShopId { get; set; }
        public virtual ProductoPetShop Producto { get; set; }
    }
}
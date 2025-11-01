using System;
using System.ComponentModel.DataAnnotations; // <--- Añade este using

namespace PetConnect.Models
{
    public class Comentario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del autor es obligatorio.")] // Validación
        public required string Autor { get; set; }

        [Required(ErrorMessage = "El texto del comentario es obligatorio.")] // Validación
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El comentario debe tener entre 3 y 500 caracteres.")] // Validación de longitud
        public string AutorId { get; set; } // El ID de Identity del autor
        public string AutorFotoUrl { get; set; } // La URL de la foto en ese momento
        public required string Texto { get; set; }

        public DateTime FechaComentario { get; set; } = DateTime.UtcNow; // <-- Valor por defecto

        // Clave foránea (ya estaba bien)
        [Required]
        public int NoticiaId { get; set; }
        public Noticia Noticia { get; set; } = default!;
    }
}
// PetConnect/Models/Mascota.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio.")]
        public string Tipo { get; set; } // "Perro", "Gato", "Otro"

        [StringLength(100)]
        public string? Raza { get; set; }

        [StringLength(50)]
        public string? Edad { get; set; } // "Cachorro", "Adulto", "Senior"

        [StringLength(50)]
        public string? Genero { get; set; } // "Macho", "Hembra"

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        public string? UrlImagen { get; set; }

        [StringLength(500)]
        public string? Temperamento { get; set; } // "Juguetón, Amigable, Tímido"

        [Display(Name = "Refugio / Contacto")]
        [StringLength(100)]
        public string? Contacto { get; set; } // Nombre del refugio o persona

        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    }
}
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class Faq
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Pregunta { get; set; }

        [Required]
        public string Respuesta { get; set; } // Puede contener HTML

        [StringLength(50)]
        public string Categoria { get; set; } = "General"; // Ej: "Cuenta", "Servicios", "Pagos"

        public int Orden { get; set; } // Para controlar en qué orden aparecen
    }
}
using System.ComponentModel.DataAnnotations;

namespace PetConnect.ViewModels
{
    public class CitaViewModel
    {
        [Required]
        public int GuarderiaId { get; set; }

        [Required]
        public string NombreCliente { get; set; }

        [Required]
        [EmailAddress]
        public string EmailCliente { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public string? Mensaje { get; set; }
    }
}
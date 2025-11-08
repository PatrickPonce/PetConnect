using System.ComponentModel.DataAnnotations;

namespace PetConnect.ViewModels
{
    public class CitaViewModel
    {
        [Required]
        public int GuarderiaId { get; set; }

        [Required(ErrorMessage = "Tu nombre es requerido.")]
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "Tu email es requerido.")]
        [EmailAddress(ErrorMessage = "Por favor, introduce un email válido.")]
        public string EmailCliente { get; set; }

        [Required(ErrorMessage = "La fecha es requerida.")]
        public DateTime Fecha { get; set; }

        public string? Mensaje { get; set; }
    }
}
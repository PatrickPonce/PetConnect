// Archivo: ViewModels/CitaViewModel.cs
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
        [EmailAddress]
        public string EmailCliente { get; set; }

        [Required(ErrorMessage = "La fecha es requerida.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es requerida.")] // <-- NUEVO
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; } // <-- NUEVO

        public string? Mensaje { get; set; }
    }
}
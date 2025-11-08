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
        [EmailAddress(ErrorMessage = "Por favor, introduce un email válido.")]
        public string EmailCliente { get; set; }

        [Required(ErrorMessage = "La fecha es requerida.")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "No puedes agendar una cita en una fecha pasada.")] // <-- VALIDACIÓN AÑADIDA
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es requerida.")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        public string? Mensaje { get; set; }
    }

    // Atributo de validación personalizado para asegurar que la fecha no sea en el pasado.
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                // La fecha seleccionada debe ser hoy o una fecha futura.
                return dateTime.Date >= DateTime.Now.Date;
            }
            return false;
        }
    }
}
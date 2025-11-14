using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PetConnect.Models
{
    [Table("pagos")]
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty; // Clave foránea al usuario
        public IdentityUser Usuario { get; set; } = null!;

        [Required]
        public string StripeSessionId { get; set; } = string.Empty; // ID de la sesión de Checkout de Stripe

        public int? ServicioId { get; set; } // Opcional: El servicio por el que se pagó
        public Servicio? Servicio { get; set; }

        [Required]
        public decimal Monto { get; set; } // Monto pagado

        [Required]
        public string Moneda { get; set; } = string.Empty; // "usd", "pen", etc.

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public string Estado { get; set; } = string.Empty; // "completado", "fallido"

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
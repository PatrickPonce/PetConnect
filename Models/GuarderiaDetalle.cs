using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models
{
    [Table("guarderia_detalles")]
    public class GuarderiaDetalle
    {
        public int Id { get; set; }
        public string? DireccionCompleta { get; set; }
        public string? Telefono { get; set; }
        public string? Descripcion { get; set; }
        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
    }
}
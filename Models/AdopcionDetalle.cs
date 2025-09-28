using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models
{
    [Table("adopcion_detalles")]
    public class AdopcionDetalle
    {
        public int Id { get; set; }
        public string DescripcionLarga { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? VideoUrl { get; set; } 
        
        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
    }
}
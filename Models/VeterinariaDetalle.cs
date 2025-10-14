using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace PetConnect.Models
{   
    [Table("veterinaria_detalles")]
    public class VeterinariaDetalle
    {
        public int Id { get; set; }
        public string DescripcionLarga { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Horario { get; set; }

        public string? TelefonoSecundario { get; set; } 
        public string? FacebookUrl { get; set; }

        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;

        public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
    }
}
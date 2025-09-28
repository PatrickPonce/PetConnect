using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace PetConnect.Models
{   
    [Table("resenas")]
    public class Resena
    {
        public int Id { get; set; }
        public string Autor { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public int Puntuacion { get; set; }
        public DateTime FechaResena { get; set; }

        public int VeterinariaDetalleId { get; set; }
        public VeterinariaDetalle VeterinariaDetalle { get; set; } = null!;

    }
}
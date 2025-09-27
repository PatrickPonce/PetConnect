using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Models
{
    public class Resena
    {
        public string Autor { get; set; }
        public string Texto { get; set; }
        public int Puntuacion { get; set; } // Puntuación de 1 a 5
        public DateTime FechaResena { get; set; }
    
    }
}
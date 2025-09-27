using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Models
{
    public class Servicio
    {

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string DescripcionCorta { get; set; }
        public string DescripcionLarga { get; set; } // Usaremos HTML aquí
        public string UrlImagen { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Horario { get; set; }
        public List<Resena> Resenas { get; set; } = new List<Resena>();
    


    }
}
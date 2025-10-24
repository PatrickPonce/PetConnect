using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Models.Api
{
    public class AnimalViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string UrlImagen { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Temperamento { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; 
    }
}
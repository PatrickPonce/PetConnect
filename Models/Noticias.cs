using System.Collections.Generic;
using PetConnect.Models;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class Noticia
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Contenido { get; set; }
        public required string UrlImagen { get; set; }
        public DateTime FechaPublicacion { get; set; } // <-- ¡Corregido!
        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
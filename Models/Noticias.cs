using System.Collections.Generic;
using PetConnect.Models;
using System.ComponentModel.DataAnnotations;


namespace PetConnect.Models
{
    public class Noticia
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo Título es obligatorio.")]
        [Display(Name = "Título")] 
        public string Titulo { get; set; }
        [Required(ErrorMessage = "El campo Contenido es obligatorio.")]
        [Display(Name = "Contenido")]
        public required string Contenido { get; set; }
        [Required(ErrorMessage = "El campo URL de Imagen es obligatorio.")]
        [Display(Name = "URL de Imagen")]
        public string UrlImagen { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool EsFijada { get; set; } = false;
        public int Vistas { get; set; } = 0;
        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
        
    }
}
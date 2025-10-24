using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class Guarderia
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public string DireccionCompleta { get; set; }
        public string Telefono { get; set; }
        public string UrlLogo { get; set; }
        public string UrlImagenPrincipal { get; set; }
        public double Calificacion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string UrlFacebook { get; set; }
        public string UrlInstagram { get; set; }

        public virtual ICollection<ComentarioGuarderia> Comentarios { get; set; }
        public virtual ICollection<FavoritoGuarderia> Favoritos { get; set; }
    }
}
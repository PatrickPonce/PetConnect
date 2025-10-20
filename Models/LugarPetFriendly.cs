using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace PetConnect.Models
{
    public class LugarPetFriendly
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } // "Cafés", "Restaurantes"

        [Required]
        [StringLength(100)]
        public string Ubicacion { get; set; } // El distrito, ej: "Jesús María"


        [Required]
        public string UrlImagenPrincipal { get; set; } // La imagen grande o de portada

        [Url]
        public string? UrlLogo { get; set; } // URL para el logo circular

        [Required]
        [StringLength(200)]
        public string DireccionCompleta { get; set; } // Ej: "Av. Húsares de Junín 561"
        
        [Range(0, 5)]
        public double Calificacion { get; set; } // De 0 a 5 estrellas

        [Phone]
        public string? Telefono { get; set; }

        [Url]
        public string? UrlFacebook { get; set; }

        [Url]
        public string? UrlInstagram { get; set; }
        
        [Required]
        public string Descripcion { get; set; }


        public ICollection<ComentarioLugar> Comentarios { get; set; } = new List<ComentarioLugar>();
        public ICollection<FavoritoLugar> Favoritos { get; set; } = new List<FavoritoLugar>();
    }
}
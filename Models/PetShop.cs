using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class PetShop
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string Descripcion { get; set; } // Descripción general
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

        // --- Campos para la sección "Características" ---
        public string MarcasPrincipales { get; set; }
        public string Categorias { get; set; }
        public bool CompraOnline { get; set; }

        public virtual ICollection<ComentarioPetShop> Comentarios { get; set; }
        public virtual ICollection<FavoritoPetShop> Favoritos { get; set; }
    }
}
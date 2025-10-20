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
        public string Categoria { get; set; }

        [Required]
        [StringLength(100)]
        public string Ubicacion { get; set; }

        [Required]
        public string UrlImagen { get; set; }

        [Required]
        public string Descripcion { get; set; }

        // Relación: Un lugar puede tener muchos comentarios
        public ICollection<ComentarioLugar> Comentarios { get; set; } = new List<ComentarioLugar>();

        // Relación: Un lugar puede ser favorito de muchos usuarios
        public ICollection<FavoritoLugar> Favoritos { get; set; } = new List<FavoritoLugar>();
    }
}
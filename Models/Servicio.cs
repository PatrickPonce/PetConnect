using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models
{
    [Table("servicios")]
    public class Servicio
    {

        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public TipoServicio Tipo { get; set; }
        public string ImagenPrincipalUrl { get; set; } = string.Empty;
        public string? FundacionNombre { get; set; }

        public string DescripcionCorta { get; set; } = string.Empty; 

        public AdopcionDetalle? AdopcionDetalle { get; set; }

        public VeterinariaDetalle? VeterinariaDetalle { get; set; }

        public PetShopDetalle? PetShopDetalle { get; set; }

        public LugarPetFriendlyDetalle? LugarPetFriendlyDetalle { get; set; }
        public GuarderiaDetalle? GuarderiaDetalle { get; set; }

        public virtual ICollection<ComentarioServicio> Comentarios { get; set; } = new List<ComentarioServicio>();
    
        public virtual ICollection<FavoritoServicio> Favoritos { get; set; } = new List<FavoritoServicio>();
    
    }
}
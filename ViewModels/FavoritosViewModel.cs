using PetConnect.Models;
using System.Collections.Generic;

namespace PetConnect.ViewModels
{
    public class FavoritosViewModel
    {
        public List<LugarPetFriendly> LugaresFavoritos { get; set; }
        public List<Guarderia> GuarderiasFavoritas { get; set; }

        public List<Noticia> NoticiasFavoritas { get; set; }

        public List<ProductoPetShop> ProductosFavoritos { get; set; }
        public List<Servicio> ServiciosFavoritos { get; set; }

        public FavoritosViewModel()
        {
            LugaresFavoritos = new List<LugarPetFriendly>();
            GuarderiasFavoritas = new List<Guarderia>();
            NoticiasFavoritas = new List<Noticia>();
            ProductosFavoritos = new List<ProductoPetShop>();
            ServiciosFavoritos = new List<Servicio>();
        }
    }
}
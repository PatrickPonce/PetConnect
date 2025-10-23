using PetConnect.Models;
using System.Collections.Generic;

namespace PetConnect.ViewModels
{
    public class FavoritosViewModel
    {
        public List<LugarPetFriendly> LugaresFavoritos { get; set; }


        public FavoritosViewModel()
        {
            LugaresFavoritos = new List<LugarPetFriendly>();

        }
    }
}
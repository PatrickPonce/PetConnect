using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PetConnect.Models;

namespace PetConnect.ViewModels
{
    public class LugarViewModel
    {
        public LugarPetFriendly Lugar { get; set; }
        public bool EsFavorito { get; set; }
    }
}
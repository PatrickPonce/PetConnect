using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;



namespace PetConnect.Models
{
    public class FavoritoLugar
    {
        // Foreign Key para el LugarPetFriendly
        public int LugarPetFriendlyId { get; set; }
        public virtual LugarPetFriendly LugarPetFriendly { get; set; }
        
        // Foreign Key para el Usuario
        public string UsuarioId { get; set; }
        public virtual IdentityUser Usuario { get; set; }
    }
}
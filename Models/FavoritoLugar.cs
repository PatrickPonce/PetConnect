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
        public int Id { get; set; }

        // Foreign Key para el Usuario
        public string UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public IdentityUser Usuario { get; set; }

        // Foreign Key para el LugarPetFriendly
        public int LugarPetFriendlyId { get; set; }
        public LugarPetFriendly LugarPetFriendly { get; set; }
    }
}
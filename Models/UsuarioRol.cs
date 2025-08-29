using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models
{
    [Table("UsuarioRoles")]
    public class UsuarioRol
    {
        public int UsuarioId { get; set; }
        public int RolId { get; set; }

        
        public Usuario? Usuario { get; set; }
        public Rol? Rol { get; set; }
    }
}
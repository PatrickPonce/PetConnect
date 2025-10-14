using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models
{
    [Table("petshop_detalles")]
    public class PetShopDetalle
    {
        public int Id { get; set; }
        public string DescripcionLarga { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        
        public string? MarcasDestacadas { get; set; }
        public string? CategoriasProductos { get; set; }
        public bool OfreceCompraOnline { get; set; }

        public int ServicioId { get; set; }
        public Servicio Servicio { get; set; } = null!;
    }
}
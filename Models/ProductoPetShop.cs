// Models/ProductoPetShop.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PetConnect.Models
{
    public class ProductoPetShop
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }

        [NotMapped] // No guardaremos esto en la BD, vendrá de la API de Pexels
        public string UrlImagen { get; set; } = string.Empty;

        public decimal Precio { get; set; }
        public List<string> Tags { get; set; } = new List<string>(); // Etiquetas
        public required string TipoProducto { get; set; } // "Comida", "Juguetes", etc.
        public required string QueryImagen { get; set; } // Término de búsqueda para Pexels

        // Relación con favoritos
        public ICollection<FavoritoProducto> Favoritos { get; set; } = new List<FavoritoProducto>();

        public ICollection<ResenaProducto> Resenas { get; set; } = new List<ResenaProducto>();

        // --- NUEVO: Propiedad calculada para el promedio de estrellas ---
        [NotMapped]
        public double PromedioCalificacion => Resenas.Any() ? Resenas.Average(r => r.Puntuacion) : 0;
    }
}
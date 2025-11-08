using PetConnect.Models;
using System.Collections.Generic;

namespace PetConnect.ViewModels
{
    public class ProductoViewModel
    {
        // --- Datos Comunes ---
        public string Nombre { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // --- Solo para Productos Internos (BD) ---
        public ProductoPetShop? Producto { get; set; }
        public int Id { get; set; }
        public bool EsFavorito { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        // --- Solo para Productos Externos (Google) ---
        public bool EsExterno { get; set; } = false; // Por defecto es interno
        public string UrlExterna { get; set; } = string.Empty; // Enlace a la tienda real
        public string NombreTienda { get; set; } = string.Empty;
    }
}
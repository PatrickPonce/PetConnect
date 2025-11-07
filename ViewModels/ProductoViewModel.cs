// ViewModels/ProductoViewModel.cs
using PetConnect.Models; // Asegúrate que esto apunte a tus modelos

namespace PetConnect.ViewModels
{
    public class ProductoViewModel
    {
        public ProductoPetShop Producto { get; set; }
        public bool EsFavorito { get; set; }
    }
}
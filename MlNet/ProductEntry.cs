// MlNet/ProductEntry.cs
using Microsoft.ML.Data;

namespace PetConnect.MlNet
{
    public class ProductEntry
    {
        [LoadColumn(0)]
        public string UserId { get; set; }

        [LoadColumn(1)]
        public int ProductId { get; set; }

        [LoadColumn(2)]
        public float Label { get; set; } // 1 = Favorito (lo usaremos como "rating" implícito)
    }
}
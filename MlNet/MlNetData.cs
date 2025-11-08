using Microsoft.ML.Data;
using System;

namespace PetConnect.MlNet
{
    // [REGLA ML.NET]: Las propiedades deben ser públicas y tener los atributos correctos.
    public class NoticiaData
    {
        // El nombre de la columna que usaremos para predecir (Features)
        [LoadColumn(0), ColumnName("Content")] // <-- Añadido ColumnName
        public string Content { get; set; } = string.Empty;

        // El nombre de la columna que usaremos como etiqueta (Label)
        [LoadColumn(1), ColumnName("Tag")] // <-- Añadido ColumnName
        public string Tag { get; set; } = string.Empty;
    }

    public class NoticiaPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedTag { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }
    public class NoticiaRatingInput
    {
       
        [LoadColumn(0)]
        public string UsuarioId { get; set; } = ""; // ID de IdentityUser (ej: "abc-123")

        [LoadColumn(1)]
        public int NoticiaId { get; set; } // ID de la Noticia (ej: 1, 2, 3)

        [LoadColumn(2)]
        public float Label { get; set; } // La "calificación". Usaremos 1.0f para "Favorito"
    }

    // 2. CLASE DE PREDICCIÓN: Qué nos devuelve el modelo
    public class NoticiaRatingPrediction
    {
        // La calificación predicha (ej: 0.9 = "le gustaría", 0.1 = "no le gustaría")
        public float Score { get; set; }
    }
}
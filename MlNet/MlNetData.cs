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
}
using Microsoft.Extensions.Logging;
using PetConnect.MlNet; 
using System;
using System.IO;
using Microsoft.Extensions.ML;

namespace PetConnect.Services
{
    // Usamos IPredictionEnginePool que configuramos en Program.cs
    public class MlNetPredictionService
    {
        private readonly Microsoft.Extensions.ML.PredictionEnginePool<NoticiaData, NoticiaPrediction> _predictionEnginePool;
        private readonly ILogger<MlNetPredictionService> _logger;

        public MlNetPredictionService(
            Microsoft.Extensions.ML.PredictionEnginePool<NoticiaData, NoticiaPrediction> predictionEnginePool,
            ILogger<MlNetPredictionService> logger)
        {
            _predictionEnginePool = predictionEnginePool;
            _logger = logger;

            // Verificamos si el modelo se cargó (el archivo .zip es opcional)
            if (File.Exists("TextClassificationModel.zip"))
            {
                _logger.LogInformation("ML.NET PredictionEnginePool cargado correctamente.");
            }
            else
            {
                _logger.LogWarning("ML.NET: No se encontró el archivo TextClassificationModel.zip. Las predicciones fallarán.");
            }
        }

        // Método principal para predecir la categoría de una noticia
        public string PredecirCategoria(string titulo, string contenido)
        {
            if (!File.Exists("TextClassificationModel.zip"))
            {
                return "Modelo no disponible"; 
            }

            // 1. Prepara los datos de entrada
            var input = new NoticiaData
            {
                // Combina Titulo y Contenido, como hicimos en el entrenamiento
                Content = $"{titulo} {contenido}" 
            };

            try
            {
                // 2. Realiza la predicción usando el pool de ML.NET
                var prediction = _predictionEnginePool.Predict(input as NoticiaData);

                // 3. Devuelve la etiqueta más probable
                return prediction.PredictedTag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar la predicción de ML.NET.");
                return "Error en predicción";
            }
        }
    }
}
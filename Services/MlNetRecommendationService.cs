using Microsoft.Extensions.ML;
using Microsoft.Extensions.Logging;
using PetConnect.MlNet; // Contiene las nuevas clases de Rating
using PetConnect.Data; // Para el DbContext
using PetConnect.Models; // Para Noticia
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PetConnect.Services
{
    public class MlNetRecommendationService
    {
        private readonly PredictionEnginePool<NoticiaRatingInput, NoticiaRatingPrediction> _predictionPool;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MlNetRecommendationService> _logger;
        private static List<int>? _allNoticiaIdsCache; // Caché simple de IDs

        public MlNetRecommendationService(
            PredictionEnginePool<NoticiaRatingInput, NoticiaRatingPrediction> predictionPool,
            ApplicationDbContext context,
            ILogger<MlNetRecommendationService> logger)
        {
            _predictionPool = predictionPool;
            _context = context;
            _logger = logger;
            
            // Cargar los IDs de noticias en caché al iniciar (si no existen)
            if (_allNoticiaIdsCache == null)
            {
                _logger.LogInformation("Inicializando caché de IDs de noticias para recomendador.");
                _allNoticiaIdsCache = _context.Noticias.Select(n => n.Id).ToList();
            }
        }

        // El método principal que llamará tu Vista
        public List<Noticia> GetRecommendations(string userId, int currentNoticiaId, int topN = 5)
        {
            if (!File.Exists("RecommendationModel.zip"))
            {
                _logger.LogWarning("RecommendationModel.zip no encontrado. Omitiendo recomendaciones.");
                return new List<Noticia>(); // Devuelve lista vacía si el modelo no existe
            }

            _logger.LogInformation($"Generando {topN} recomendaciones para Usuario: {userId}, excluyendo Noticia: {currentNoticiaId}");

            var input = new NoticiaRatingInput { UsuarioId = userId };
            var predictions = new List<(int NoticiaId, float Score)>();

            // 1. Iterar sobre TODAS las noticias (excepto la actual) y predecir un score
            foreach (var noticiaId in _allNoticiaIdsCache ?? new List<int>())
            {
                if (noticiaId == currentNoticiaId)
                {
                    continue; // No recomendar la noticia que ya está viendo
                }

                input.NoticiaId = noticiaId;

                // 2. Predecir el "Score" (qué tanto le gustaría)
                var prediction = _predictionPool.Predict(input);
                
                // Solo añadir si el score es positivo (ML.NET a veces da negativos)
                if (prediction.Score > 0)
                {
                    predictions.Add((noticiaId, prediction.Score));
                }
            }

            // 3. Ordenar por el score más alto y tomar las Top N
            var topNoticiaIds = predictions.OrderByDescending(p => p.Score)
                                           .Take(topN)
                                           .Select(p => p.NoticiaId)
                                           .ToList();

            if (!topNoticiaIds.Any())
            {
                _logger.LogInformation("No se generaron recomendaciones (scores bajos o sin datos).");
                return new List<Noticia>();
            }

            // 4. Consultar la base de datos UNA SOLA VEZ con los IDs ganadores
            var recommendedNoticias = _context.Noticias
                .Where(n => topNoticiaIds.Contains(n.Id))
                .ToList()
                .OrderBy(n => topNoticiaIds.IndexOf(n.Id)); // Mantiene el orden del score

            return recommendedNoticias.ToList();
        }
    }
}
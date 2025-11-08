using Microsoft.ML;
using Microsoft.ML.Trainers;
using PetConnect.Data;
using PetConnect.MlNet; // Usamos tu nueva carpeta
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace PetConnect.Services
{
    public class RecommendationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ITransformer _model;
        private readonly MLContext _mlContext;
        private readonly string _modelPath;

        public RecommendationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _mlContext = new MLContext();
            _modelPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "MLModels", "ProductRecommender.zip");
        }

        // Método para entrenar el modelo
        public async Task TrainModel()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // 1. Cargar datos: Obtenemos todos los favoritos
                var favoritos = await context.FavoritosProducto.AsNoTracking().ToListAsync();
                
                // Necesitamos datos suficientes para entrenar
                if (favoritos.Count < 5) return; 

                var trainingData = favoritos.Select(f => new ProductEntry
                {
                    UserId = f.UsuarioId,
                    ProductId = f.ProductoPetShopId,
                    Label = 1 // "Le gusta"
                }).ToList();

                var trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                // 2. Configurar el pipeline de entrenamiento
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: nameof(ProductEntry.UserId))
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "ProductIdEncoded", inputColumnName: nameof(ProductEntry.ProductId)))
                    .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options: new MatrixFactorizationTrainer.Options
                    {
                        MatrixColumnIndexColumnName = "UserIdEncoded",
                        MatrixRowIndexColumnName = "ProductIdEncoded",
                        LabelColumnName = "Label",
                        NumberOfIterations = 20,
                        ApproximationRank = 100
                    }));

                // 3. Entrenar y guardar el modelo
                _model = pipeline.Fit(trainingDataView);
                
                // Asegurar que el directorio existe
                var modelDir = Path.GetDirectoryName(_modelPath);
                if (!Directory.Exists(modelDir)) Directory.CreateDirectory(modelDir);

                _mlContext.Model.Save(_model, trainingDataView.Schema, _modelPath);
            }
        }

        // Método para obtener recomendaciones
        public async Task<List<int>> GetRecommendationsAsync(string userId, int topN = 3)
        {
            // Intentar cargar el modelo si no está en memoria
            if (_model == null)
            {
                if (File.Exists(_modelPath))
                {
                    _model = _mlContext.Model.Load(_modelPath, out _);
                }
                else
                {
                    // Si no existe, intentamos entrenarlo ahora mismo
                    await TrainModel();
                    if (_model == null) return new List<int>(); // Si sigue sin haber modelo (ej. sin datos), retorna vacío
                }
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Obtener productos que el usuario AÚN NO tiene en favoritos
                var allProductIds = await context.ProductosPetShop.Select(p => p.Id).ToListAsync();
                var userFavorites = await context.FavoritosProducto
                    .Where(f => f.UsuarioId == userId)
                    .Select(f => f.ProductoPetShopId)
                    .ToListAsync();

                var candidateProducts = allProductIds.Except(userFavorites).ToList();

                // 2. Predecir el "score" para cada candidato
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductEntry, ProductPrediction>(_model);
                var ratings = new List<(int ProductId, float Score)>();

                foreach (var productId in candidateProducts)
                {
                    try 
                    {
                        var prediction = predictionEngine.Predict(new ProductEntry
                        {
                            UserId = userId,
                            ProductId = productId
                        });
                        ratings.Add((productId, prediction.Score));
                    }
                    catch
                    {
                        // Puede fallar si el usuario/producto es nuevo y no estaba en el entrenamiento
                        // Ignoramos este error en la predicción
                    }
                }

                // 3. Devolver los Top N con mayor puntaje
                return ratings.OrderByDescending(r => r.Score)
                              .Take(topN)
                              .Select(r => r.ProductId)
                              .ToList();
            }
        }
    }
}
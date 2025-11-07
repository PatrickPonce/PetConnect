// Services/ChatbotService.cs
using Google.Cloud.AIPlatform.V1;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;

namespace PetConnect.Services
{
    public class ChatbotService
    {
        private readonly PredictionServiceClient _predictionServiceClient;
        private readonly string _endpoint;
        private readonly string _projectId;

        public ChatbotService(IConfiguration configuration)
        {
            _projectId = configuration["Gemini:ProjectId"];
            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("El ProjectId o ApiKey de Gemini no están configurados.");
            }
            
            _endpoint = $"projects/{_projectId}/locations/us-central1/publishers/google/models/gemini-1.0-pro";

            _predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = "us-central1-aiplatform.googleapis.com",
                GrpcAdapter = GrpcNetClientAdapter.Default.WithAdditionalOptions(options => options.CallOptions = new Grpc.Core.CallOptions(
                    new Grpc.Core.Metadata { { "x-goog-api-key", apiKey } }
                ))
            }.Build();
        }

        public async Task<string> GenerarRespuestaAsync(string promptUsuario)
        {
            var systemPrompt = @"Eres 'Patitas', el asistente virtual experto de PetConnect, una plataforma para amantes de las mascotas en Perú. 
                Tu especialidad son las veterinarias, adopción, pet shops y lugares pet-friendly.
                Tu tono debe ser amigable, servicial y empático. Usa emojis de animales cuando sea apropiado 🐾.
                NO debes dar consejos médicos veterinarios. Si te preguntan por síntomas, debes recomendar SIEMPRE 'consultar con una veterinaria profesional de nuestro directorio'.
                Basa tus respuestas en el contexto de Perú. Si no sabes una respuesta, di que estás aprendiendo y que recomendarías buscar en el directorio del sitio.";

            var fullPrompt = $"{systemPrompt}\n\nUsuario: {promptUsuario}\nPatitas:";

            var request = new PredictRequest
            {
                Endpoint = _endpoint,
                Instances =
                {
                    new Google.Protobuf.WellKnownTypes.Value
                    {
                        StructValue = new Google.Protobuf.WellKnownTypes.Struct
                        {
                            Fields = { { "prompt", Google.Protobuf.WellKnownTypes.Value.ForString(fullPrompt) } }
                        }
                    }
                },
                Parameters = Google.Protobuf.WellKnownTypes.Value.ForStruct(new Google.Protobuf.WellKnownTypes.Struct
                {
                    Fields =
                    {
                        { "temperature", Google.Protobuf.WellKnownTypes.Value.ForNumber(0.3) },
                        { "maxOutputTokens", Google.Protobuf.WellKnownTypes.Value.ForNumber(256) },
                        { "topP", Google.Protobuf.WellKnownTypes.Value.ForNumber(0.8) },
                        { "topK", Google.Protobuf.WellKnownTypes.Value.ForNumber(40) }
                    }
                })
            };

            try
            {
                var response = await _predictionServiceClient.PredictAsync(request);
                var content = response.Predictions[0].StructValue.Fields["content"].StringValue;
                return content;
            }
            catch (System.Exception ex)
            {
                return "¡Uy! Parece que mis circuitos se enredaron 🐾. Intenta preguntarme de nuevo en un momento.";
            }
        }
    }
}
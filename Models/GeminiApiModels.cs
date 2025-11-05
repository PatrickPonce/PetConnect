// EN: Models/GeminiApiModels.cs

namespace PetConnect.Models
{
    // --- Modelos para la SOLICITUD a Gemini ---
    public class GeminiApiRequest
    {
        public System.Collections.Generic.List<GeminiContent> Contents { get; set; }
    }
    public class GeminiContent
    {
        public System.Collections.Generic.List<GeminiPart> Parts { get; set; }
    }
    public class GeminiPart
    {
        public string Text { get; set; }
    }

    // --- Modelos para la RESPUESTA de Gemini ---
    public class GeminiApiResponse
    {
        public System.Collections.Generic.List<GeminiCandidate> Candidates { get; set; }
    }
    public class GeminiCandidate
    {
        public GeminiContent Content { get; set; }
    }

    // --- Modelo para la Solicitud del Controlador ---
    // (Esta es la clase que te daba el error en la imagen 'image_ba5a07.png')
    public class GenerarArticuloRequest
    {
        public string Idea { get; set; }
    }
}
// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // <-- AÑADE ESTA DIRECTIVA 'USING'
using PetConnect.Models;
using System.Net.Http; // <-- AÑADE ESTE USING
using System.Threading.Tasks;

namespace PetConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager; // <-- AÑADE ESTA LÍNEA

        private readonly IConfiguration _configuration;

        // Modifica el constructor para que también reciba SignInManager
        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _logger = logger;
            _signInManager = signInManager; // <-- AÑADE ESTA LÍNEA
            _configuration = configuration;
        }

        // --- ACCIÓN INDEX MODIFICADA ---
        public IActionResult Index()
        {
            // Verificamos si el usuario ya ha iniciado sesión
            if (_signInManager.IsSignedIn(User))
            {
                // Si ya está autenticado, no le mostramos la landing page.
                // Lo redirigimos a la página principal de la aplicación (la de "Guest").
                return RedirectToAction("Guest");
            }

            // Si no ha iniciado sesión, entonces sí mostramos la landing page.
            return View(); // Devuelve /Views/Home/Index.cshtml
        }

        public IActionResult Guest()
        {
            // Esta será ahora la página principal para usuarios logueados e invitados.
            // Asegúrate de que esta vista (/Views/Home/Guest.cshtml) use el layout principal (_Layout.cshtml).
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet("debug/gemini-models")]
        public async Task<IActionResult> DebugGeminiModels()
        {
            // Usa la misma clave que tu chatbot para asegurar que la prueba sea válida
            var apiKey = _configuration["MiGemini:ApiKey"]; 
            if (string.IsNullOrEmpty(apiKey))
            {
                return Content("Error: La clave 'MiGemini:ApiKey' no está configurada.");
            }

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
            var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                var content = await response.Content.ReadAsStringAsync();
                
                // Devolvemos el JSON crudo que nos da Google para que lo veas
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return Content($"Error al conectar con la API de Gemini: {ex.Message}");
            }
        }
    }
}
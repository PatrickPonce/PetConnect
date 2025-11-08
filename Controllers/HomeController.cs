// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // <-- AÑADE ESTA DIRECTIVA 'USING'
using PetConnect.Models;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Linq; // <-- AÑADE ESTE USING
using System.Threading.Tasks;
using System.Net.Http;

namespace PetConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context; // <-- Necesario para noticias dinámicas
        private readonly IConfiguration _configuration;

        // Modifica el constructor para que también reciba SignInManager
        // Constructor combinado que resuelve el conflicto
        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _signInManager = signInManager;
            _context = context;
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

        // --- ACCIÓN GUEST ACTUALIZADA ---
        public async Task<IActionResult> Guest()
        {
            // Noticias para el carrusel
            var noticiasDestacadas = await _context.Noticias
                .OrderByDescending(n => n.FechaPublicacion)
                .Take(6) // Traemos 6 para que el carrusel se vea lleno
                .ToListAsync();

            // (Opcional) Podrías pasar contadores reales si quisieras
            // ViewBag.TotalUsuarios = await _context.Users.CountAsync();
            // ViewBag.TotalServicios = await _context.Servicios.CountAsync();

            return View(noticiasDestacadas);
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

        // --- NUEVAS ACCIONES PARA EL FOOTER ---
        public IActionResult Terms() // Términos y Condiciones
        {
            return View();
        }

        public IActionResult Contact() // Contacto
        {
            return View();
        }

        public async Task<IActionResult> Faq()
        {
            // Obtenemos todas las FAQs ordenadas por categoría y luego por orden
            var faqs = await _context.Faqs
                .OrderBy(f => f.Categoria)
                .ThenBy(f => f.Orden)
                .ToListAsync();

            return View(faqs);
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
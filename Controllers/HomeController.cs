// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // <-- AÑADE ESTA DIRECTIVA 'USING'
using PetConnect.Models;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager; // <-- AÑADE ESTA LÍNEA
        private readonly ApplicationDbContext _context;

        // Modifica el constructor para que también reciba SignInManager
        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            _logger = logger;
            _signInManager = signInManager; // <-- AÑADE ESTA LÍNEA
            _context = context;
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
        public IActionResult Faq() // Preguntas Frecuentes
        {
            return View();
        }

        public IActionResult Terms() // Términos y Condiciones
        {
            return View();
        }

        public IActionResult Contact() // Contacto
        {
            return View();
        }
        // ------------------------------------
    }
}
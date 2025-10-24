// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // <-- AÑADE ESTA DIRECTIVA 'USING'
using PetConnect.Models;

namespace PetConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager; // <-- AÑADE ESTA LÍNEA

        // Modifica el constructor para que también reciba SignInManager
        public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _signInManager = signInManager; // <-- AÑADE ESTA LÍNEA
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
    }
}
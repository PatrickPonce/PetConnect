// Controllers/ServicioController.cs
using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers
{
    public class ServicioController : Controller
    {
        // Esta acción simplemente muestra la página con las 5 tarjetas de categorías.
        // El contenido es estático y vive en la vista.
        public IActionResult Index()
        {
            return View();
        }
    }
}
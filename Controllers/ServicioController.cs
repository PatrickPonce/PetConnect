using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers
{
    public class ServicioController : Controller
    {
        public IActionResult Detalle(int id = 0)
        {
            ViewData["Id"] = id;
            return View();
        }
    }
}

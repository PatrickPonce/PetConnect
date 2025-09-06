using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers
{
    public class DirectorioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

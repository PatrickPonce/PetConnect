using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers
{
    // Restringe el acceso a este controlador solo a usuarios con el rol "Admin"
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult ConfigPage()
        {
            return View();
        }
    }
}
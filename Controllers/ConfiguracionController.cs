// Controllers/ConfiguracionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ConfiguracionController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
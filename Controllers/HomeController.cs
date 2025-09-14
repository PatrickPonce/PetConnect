using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetConnect.Data;
using PetConnect.Models;

namespace PetConnect.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    // --- LÍNEA CORREGIDA ---
    // Los nombres de los tipos (ILogger, HomeController, ApplicationDbContext)
    // deben coincidir exactamente con cómo se definieron las clases.
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
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
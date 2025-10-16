using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetConnect.Data;   
using PetConnect.Models;
using Microsoft.EntityFrameworkCore;  
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

[Authorize(Roles = "Admin")]
public class ConfiguracionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ConfiguracionController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var configuraciones = await _context.ConfiguracionesSitio.ToDictionaryAsync(c => c.Clave, c => c.Valor);
        ViewBag.Configuraciones = configuraciones;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(string nombreSitio, IFormFile logo, IFormFile banner)
    {
        await GuardarConfiguracion("NombreSitio", nombreSitio);

        if (logo != null && logo.Length > 0)
        {
            string urlLogo = await GuardarImagen(logo, "logo");
            await GuardarConfiguracion("UrlLogo", urlLogo);
            await GuardarConfiguracion("UrlLogoVersion", DateTime.Now.Ticks.ToString());
        }

        if (banner != null && banner.Length > 0)
        {
            string urlBanner = await GuardarImagen(banner, "banner");
            await GuardarConfiguracion("UrlBanner", urlBanner);
            await GuardarConfiguracion("UrlBannerVersion", DateTime.Now.Ticks.ToString());
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "¡Configuración guardada con éxito!";
        return RedirectToAction("Index");
    }

    private async Task GuardarConfiguracion(string clave, string valor)
    {
        var config = await _context.ConfiguracionesSitio.FirstOrDefaultAsync(c => c.Clave == clave);
        if (config != null)
        {
            config.Valor = valor;
        }
        else
        {
            _context.ConfiguracionesSitio.Add(new ConfiguracionSitio { Clave = clave, Valor = valor });
        }
    }

    private async Task<string> GuardarImagen(IFormFile archivo, string nombreBase)
    {
        string wwwRootPath = _hostEnvironment.WebRootPath;
        string carpetaDestino = Path.Combine(wwwRootPath, "images");

        string extension = Path.GetExtension(archivo.FileName);
        string nombreArchivo = $"{nombreBase}{extension}";
        string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

        using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivo.CopyToAsync(fileStream);
        }

        return $"/images/{nombreArchivo}";
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetConnect.Data;   
using PetConnect.Models;
using Microsoft.EntityFrameworkCore;  
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using PetConnect.Services;

[Authorize(Roles = "Admin")]
public class ConfiguracionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ConfiguracionSitioService _configService;

    public ConfiguracionController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, ConfiguracionSitioService configService)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
        _configService = configService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ColorPrincipal = await _configService.ObtenerValorAsync("AdminColorPrincipal", "#97a992");
        ViewBag.ColorSecundario = await _configService.ObtenerValorAsync("AdminColorSecundario", "#f3d9d7");
        var configuraciones = await _context.ConfiguracionesSitio.ToDictionaryAsync(c => c.Clave, c => c.Valor);
        ViewBag.Configuraciones = configuraciones;
        return View();
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
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
    // Pega esto dentro de tu clase ConfiguracionController

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GuardarTema(string colorPrincipal, string colorSecundario, int? noticiaId)
    {
        if (string.IsNullOrEmpty(colorPrincipal) || string.IsNullOrEmpty(colorSecundario))
        {
            TempData["ErrorMessage"] = "Los colores no pueden estar vacíos.";
        }
        else
        {
            // Llama al servicio para guardar los nuevos valores
            await _configService.GuardarValorAsync("AdminColorPrincipal", colorPrincipal);
            await _configService.GuardarValorAsync("AdminColorSecundario", colorSecundario);
            TempData["SuccessMessage"] = "Tema actualizado correctamente.";
        }
        
        // Si recibimos un noticiaId, volvemos a la página de detalle de admin
        if (noticiaId.HasValue)
        {
            return RedirectToAction("DetalleAdmin", "Noticias", new { id = noticiaId.Value });
        }
        
        // Si no, volvemos al índice de configuración
        return RedirectToAction("Index");
    }
}
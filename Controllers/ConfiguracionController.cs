using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetConnect.Data;
using PetConnect.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System;

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(string nombreSitio, IFormFile? logo, IFormFile? banner)
    {
        try
        {
            // 1. Guardar Nombre del Sitio
            if (!string.IsNullOrWhiteSpace(nombreSitio))
            {
                await GuardarConfiguracion("NombreSitio", nombreSitio.Trim());
            }

            // 2. Guardar Logo (si se subió uno nuevo)
            if (logo != null && logo.Length > 0)
            {
                if (!EsImagenValida(logo))
                {
                    TempData["ErrorMessage"] = "El archivo de Logo no es una imagen válida (solo JPG, PNG, GIF).";
                    return RedirectToAction("Index");
                }
                string urlLogo = await GuardarImagen(logo, "logo");
                await GuardarConfiguracion("UrlLogo", urlLogo);
                await GuardarConfiguracion("UrlLogoVersion", DateTime.Now.Ticks.ToString());
            }

            // 3. Guardar Banner (si se subió uno nuevo)
            if (banner != null && banner.Length > 0)
            {
                if (!EsImagenValida(banner))
                {
                    TempData["ErrorMessage"] = "El archivo de Banner no es una imagen válida (solo JPG, PNG, GIF).";
                    return RedirectToAction("Index");
                }
                string urlBanner = await GuardarImagen(banner, "banner");
                await GuardarConfiguracion("UrlBanner", urlBanner);
                await GuardarConfiguracion("UrlBannerVersion", DateTime.Now.Ticks.ToString());
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Configuración actualizada correctamente!";
        }
        catch (Exception ex)
        {
            // En un caso real, loguearíamos 'ex'
            TempData["ErrorMessage"] = "Ocurrió un error al guardar la configuración. Inténtalo de nuevo.";
        }

        return RedirectToAction("Index");
    }

    // --- MÉTODOS AUXILIARES ---

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
        string carpetaDestino = Path.Combine(wwwRootPath, "images", "config"); // Guardar en subcarpeta 'config' para orden

        if (!Directory.Exists(carpetaDestino))
        {
            Directory.CreateDirectory(carpetaDestino);
        }

        string extension = Path.GetExtension(archivo.FileName);
        // Usamos un nombre fijo para reemplazar el anterior, o podrías usar timestamps para evitar caché
        string nombreArchivo = $"{nombreBase}_custom{extension}"; 
        string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

        using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivo.CopyToAsync(fileStream);
        }

        return $"/images/config/{nombreArchivo}";
    }

    private bool EsImagenValida(IFormFile archivo)
    {
        // Validación simple por extensión y tipo MIME
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        return extensionesPermitidas.Contains(extension) && archivo.ContentType.StartsWith("image/");
    }
}
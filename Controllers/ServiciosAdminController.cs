using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using Microsoft.AspNetCore.Hosting; // Necesario para IWebHostEnvironment
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

[Authorize(Roles = "Admin")]
public class ServiciosAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment; // Para manejar archivos

    public ServiciosAdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // --- 1. LISTADO (INDEX) ---
    public async Task<IActionResult> Index(string searchString, TipoServicio? tipoFilter, int? page)
    {
        var query = _context.Servicios.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(s => s.Nombre.ToLower().Contains(searchString.ToLower()));
        }
        if (tipoFilter.HasValue)
        {
            query = query.Where(s => s.Tipo == tipoFilter.Value);
        }

        query = query.OrderBy(s => s.Tipo).ThenBy(s => s.Nombre);

        int pageSize = 10;
        int pageNumber = (page ?? 1);
        var serviciosPaginados = await query.ToPagedListAsync(pageNumber, pageSize);
        
        ViewData["CurrentFilter"] = searchString;
        ViewData["TipoFilter"] = tipoFilter;

        return View(serviciosPaginados);
    }

    // --- 2. CREAR (CREATE) ---
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Servicio servicio, IFormFile? imagenArchivo)
    {
        if (ModelState.IsValid)
        {
            // Manejo de imagen
            if (imagenArchivo != null)
            {
                servicio.ImagenPrincipalUrl = await SubirImagen(imagenArchivo);
            }

            _context.Add(servicio);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Servicio creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(servicio);
    }

    // --- 3. EDITAR (EDIT) ---
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null) return NotFound();
        
        return View(servicio);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Servicio servicio, IFormFile? nuevaImagen)
    {
        if (id != servicio.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Si subieron una nueva imagen, la guardamos y reemplazamos la URL
                if (nuevaImagen != null)
                {
                    // (Opcional: podrías borrar la imagen anterior aquí si quisieras ahorrar espacio)
                    servicio.ImagenPrincipalUrl = await SubirImagen(nuevaImagen);
                }
                // Si no subieron nada, mantenemos la URL que ya tenía (asegúrate de tener un hidden input en la vista)

                _context.Update(servicio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Servicios.Any(e => e.Id == servicio.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(servicio);
    }

    // --- 4. ELIMINAR (DELETE) ---
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio != null)
        {
            // (Opcional: borrar la imagen del servidor al eliminar el servicio)
            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Servicio eliminado correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    // --- MÉTODO AUXILIAR PARA SUBIR IMÁGENES ---
    private async Task<string> SubirImagen(IFormFile archivo)
    {
        string wwwRootPath = _hostEnvironment.WebRootPath;
        string fileName = Path.GetFileNameWithoutExtension(archivo.FileName);
        string extension = Path.GetExtension(archivo.FileName);
        
        // Generamos un nombre único para evitar colisiones
        string nuevoNombre = $"{fileName}_{DateTime.Now.Ticks}{extension}";
        string pathDestino = Path.Combine(wwwRootPath, "images", "servicios");

        // Aseguramos que la carpeta exista
        if (!Directory.Exists(pathDestino))
        {
            Directory.CreateDirectory(pathDestino);
        }

        string pathCompleto = Path.Combine(pathDestino, nuevoNombre);

        using (var fileStream = new FileStream(pathCompleto, FileMode.Create))
        {
            await archivo.CopyToAsync(fileStream);
        }

        // Devolvemos la ruta relativa para guardar en la BD
        return $"/images/servicios/{nuevoNombre}";
    }
}
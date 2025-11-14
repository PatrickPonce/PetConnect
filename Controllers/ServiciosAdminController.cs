using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

[Authorize(Roles = "Admin")]
public class ServiciosAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ServiciosAdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // --- 1. VISTA PRINCIPAL (CATEGORÍAS) ---
    public IActionResult Index()
    {
        // Esta vista solo mostrará las tarjetas de categorías, no necesita datos.
        return View();
    }

    // --- 2. LISTADO POR TIPO (TABLA) ---
    public async Task<IActionResult> Listado(TipoServicio tipo, string searchString, int? page)
    {
        // Filtramos inicialmente por el tipo de servicio seleccionado
        var query = _context.Servicios.Where(s => s.Tipo == tipo).AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(s => s.Nombre.ToLower().Contains(searchString.ToLower()));
        }

        query = query.OrderBy(s => s.Nombre);

        int pageSize = 10;
        int pageNumber = (page ?? 1);
        
        // Usamos ToPagedListAsync de X.PagedList
        var serviciosPaginados = await query.ToPagedListAsync(pageNumber, pageSize);
        
        // Pasamos datos a la vista para mantener el estado de los filtros y el título
        ViewData["CurrentFilter"] = searchString;
        ViewData["TipoActual"] = tipo;
        ViewData["Title"] = $"Gestión de {tipo}";

        return View(serviciosPaginados);
    }

    // --- 3. CREAR (CREATE) ---
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Servicio servicio, IFormFile? imagenArchivo, 
        VeterinariaDetalle veterinariaDetalle, 
        PetShopDetalle petShopDetalle, 
        AdopcionDetalle adopcionDetalle)
    {
        // Eliminamos los detalles del ModelState porque solo uno será válido,
        // y los otros generarían errores de validación innecesarios.
        ModelState.Remove("VeterinariaDetalle");
        ModelState.Remove("PetShopDetalle");
        ModelState.Remove("AdopcionDetalle");

        if (ModelState.IsValid)
        {
            // 1. Subir la imagen si existe
            if (imagenArchivo != null)
            {
                servicio.ImagenPrincipalUrl = await SubirImagen(imagenArchivo);
            }

            // 2. LÓGICA CLAVE: Asignar el objeto de detalle correcto basado en el Tipo
            switch (servicio.Tipo)
            {
                case TipoServicio.Veterinaria:
                    servicio.VeterinariaDetalle = veterinariaDetalle;
                    break;
                case TipoServicio.PetShop:
                    servicio.PetShopDetalle = petShopDetalle;
                    // El valor del checkbox llega como 'on' si está marcado
                    servicio.PetShopDetalle.OfreceCompraOnline = Request.Form.ContainsKey("PetShopDetalle.OfreceCompraOnline");
                    break;
                case TipoServicio.Adopcion:
                    servicio.AdopcionDetalle = adopcionDetalle;
                    break;
                // Añadir casos para Guarderia y LugarPetFriendly en el futuro
            }
            
            // 3. Añadir el servicio (EF Core guardará también el detalle asociado)
            _context.Add(servicio);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Servicio creado correctamente.";
            return RedirectToAction(nameof(Listado), new { tipo = servicio.Tipo });
        }

        // Si el modelo no es válido, volvemos a la vista
        return View(servicio);
    }

    // --- 4. EDITAR (EDIT) ---
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        // Incluimos los detalles posibles para que se carguen en el formulario
        var servicio = await _context.Servicios
            .Include(s => s.VeterinariaDetalle)
            .Include(s => s.PetShopDetalle)
            .Include(s => s.AdopcionDetalle)
            .FirstOrDefaultAsync(s => s.Id == id);

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
                if (nuevaImagen != null)
                {
                    servicio.ImagenPrincipalUrl = await SubirImagen(nuevaImagen);
                }
                // Si no hay nueva imagen, EF Core mantendrá la URL existente si usamos Update correctamente
                // o si la vista envía el valor en un campo oculto (recomendado).

                _context.Update(servicio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Servicios.Any(e => e.Id == servicio.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Listado), new { tipo = servicio.Tipo });
        }
        return View(servicio);
    }

    // --- 5. ELIMINAR (DELETE) ---
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio != null)
        {
            // Guardamos el tipo antes de borrar para poder redirigir
            var tipo = servicio.Tipo;
            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Servicio eliminado correctamente.";
            return RedirectToAction(nameof(Listado), new { tipo = tipo });
        }
        // Si falla, volvemos al índice principal por seguridad
        return RedirectToAction(nameof(Index));
    }

    // --- MÉTODO AUXILIAR ---
    private async Task<string> SubirImagen(IFormFile archivo)
    {
        string wwwRootPath = _hostEnvironment.WebRootPath;
        string fileName = Path.GetFileNameWithoutExtension(archivo.FileName);
        string extension = Path.GetExtension(archivo.FileName);
        string nuevoNombre = $"{fileName}_{DateTime.Now.Ticks}{extension}";
        string pathDestino = Path.Combine(wwwRootPath, "images", "servicios");

        if (!Directory.Exists(pathDestino)) Directory.CreateDirectory(pathDestino);

        using (var fileStream = new FileStream(Path.Combine(pathDestino, nuevoNombre), FileMode.Create))
        {
            await archivo.CopyToAsync(fileStream);
        }

        return $"/images/servicios/{nuevoNombre}";
    }
}
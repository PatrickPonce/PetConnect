using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using Microsoft.AspNetCore.Hosting; // Para subir imágenes
using System.IO; // Para subir imágenes
using System; // Para IFormFile

namespace PetConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Adopcion")] // Ruta limpia para el admin
    public class AdopcionAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdopcionAdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // Método para subir imagen
        private async Task<string> SubirImagen(IFormFile archivo)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(archivo.FileName);
            string extension = Path.GetExtension(archivo.FileName);
            string nuevoNombre = $"{fileName}_{DateTime.Now.Ticks}{extension}";
            string pathDestino = Path.Combine(wwwRootPath, "images", "servicios"); // Reutilizamos la carpeta

            if (!Directory.Exists(pathDestino)) Directory.CreateDirectory(pathDestino);

            using (var fileStream = new FileStream(Path.Combine(pathDestino, nuevoNombre), FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }

            return $"/images/servicios/{nuevoNombre}";
        }

        // GET: Admin/Adopcion
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.Servicios
                .Where(s => s.Tipo == TipoServicio.Adopcion) // Filtro clave
                .OrderBy(p => p.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Nombre.Contains(searchString) || p.FundacionNombre.Contains(searchString));
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedData = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedData);
        }

        // GET: Admin/Adopcion/Crear
        [Route("Crear")]
        public IActionResult Create()
        {
            // Creamos un modelo con los valores por defecto
            var model = new Servicio
            {
                Tipo = TipoServicio.Adopcion,
                AdopcionDetalle = new AdopcionDetalle() // Importante para el formulario
            };
            return View(model);
        }

        // POST: Admin/Adopcion/Crear
        [HttpPost]
        [Route("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Servicio servicio, IFormFile? imagenArchivo)
        {
            // Forzamos el tipo correcto
            servicio.Tipo = TipoServicio.Adopcion;

            // Quitamos la validación de otros detalles
            ModelState.Remove("VeterinariaDetalle");
            ModelState.Remove("PetShopDetalle");
            ModelState.Remove("LugarPetFriendlyDetalle");
            ModelState.Remove("GuarderiaDetalle");
            
            if (ModelState.IsValid)
            {
                if (imagenArchivo != null)
                {
                    servicio.ImagenPrincipalUrl = await SubirImagen(imagenArchivo);
                }

                _context.Add(servicio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Centro de Adopción creado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            // Si falla, volvemos a la vista con el modelo
            return View(servicio);
        }

        // GET: Admin/Adopcion/Editar/5
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var servicio = await _context.Servicios
                .Include(s => s.AdopcionDetalle) // Incluimos los detalles
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.Adopcion);
                
            if (servicio == null) return NotFound();
            
            if (servicio.AdopcionDetalle == null)
            {
                servicio.AdopcionDetalle = new AdopcionDetalle();
            }

            return View(servicio);
        }

        // POST: Admin/Adopcion/Editar/5
        [HttpPost]
        [Route("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? nuevaImagen)
        {
            var servicioToUpdate = await _context.Servicios
                .Include(s => s.AdopcionDetalle)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicioToUpdate == null) return NotFound();

            // Quitamos la validación de otros detalles
            ModelState.Remove("VeterinariaDetalle");
            ModelState.Remove("PetShopDetalle");
            ModelState.Remove("LugarPetFriendlyDetalle");
            ModelState.Remove("GuarderiaDetalle");

            if (await TryUpdateModelAsync<Servicio>(
                servicioToUpdate,
                "", // Prefijo vacío
                s => s.Nombre, s => s.DescripcionCorta, s => s.FundacionNombre, s => s.AdopcionDetalle))
            {
                if (nuevaImagen != null)
                {
                    servicioToUpdate.ImagenPrincipalUrl = await SubirImagen(nuevaImagen);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Centro de Adopción actualizado con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. Fue modificado por otro usuario.");
                }
            }
            
            return View(servicioToUpdate);
        }

        // GET: Admin/Adopcion/Eliminar/5
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var servicio = await _context.Servicios
                .FirstOrDefaultAsync(m => m.Id == id && m.Tipo == TipoServicio.Adopcion);
            if (servicio == null) return NotFound();
            return View(servicio);
        }

        // POST: Admin/Adopcion/Eliminar/5
        [HttpPost, ActionName("Delete")]
        [Route("Eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio != null)
            {
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Centro de Adopción eliminado con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
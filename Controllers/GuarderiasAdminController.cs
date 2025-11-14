using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace PetConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Guarderias")] // Ruta limpia para el admin
    public class GuarderiasAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuarderiasAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Guarderias
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.Guarderias.OrderBy(p => p.Nombre).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Nombre.Contains(searchString) || p.Ubicacion.Contains(searchString));
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedGuarderias = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedGuarderias);
        }

        // GET: Admin/Guarderias/Crear
        [Route("Crear")]
        public IActionResult Create()
        {
            // Modelo vacío para el formulario
            return View(new Guarderia());
        }

        // POST: Admin/Guarderias/Crear
        [HttpPost]
        [Route("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Descripcion,Ubicacion,DireccionCompleta,Telefono,UrlLogo,UrlImagenPrincipal,Calificacion,Latitud,Longitud,UrlFacebook,UrlInstagram")] 
            Guarderia guarderia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(guarderia);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Guardería creada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            // Si falla, vuelve a mostrar el formulario con los datos
            return View(guarderia);
        }

        // GET: Admin/Guarderias/Editar/5
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var guarderia = await _context.Guarderias.FindAsync(id);
            if (guarderia == null) return NotFound();
            return View(guarderia);
        }

        // POST: Admin/Guarderias/Editar/5
        [HttpPost]
        [Route("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var guarderiaToUpdate = await _context.Guarderias.FindAsync(id);
            if (guarderiaToUpdate == null) return NotFound();

            if (await TryUpdateModelAsync<Guarderia>(
                guarderiaToUpdate,
                "", // Prefijo vacío
                g => g.Nombre, g => g.Descripcion, g => g.Ubicacion, g => g.DireccionCompleta,
                g => g.Telefono, g => g.UrlLogo, g => g.UrlImagenPrincipal, g => g.Calificacion,
                g => g.Latitud, g => g.Longitud, g => g.UrlFacebook, g => g.UrlInstagram))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Guardería actualizada con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. La guardería fue modificada por otro usuario.");
                }
            }
            
            return View(guarderiaToUpdate);
        }

        // GET: Admin/Guarderias/Eliminar/5
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var guarderia = await _context.Guarderias.FirstOrDefaultAsync(m => m.Id == id);
            if (guarderia == null) return NotFound();
            return View(guarderia);
        }

        // POST: Admin/Guarderias/DeleteConfirmed/5
        [HttpPost]
        [Route("DeleteConfirmed/{id:int}")] // <-- CORRECCIÓN AQUÍ
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guarderia = await _context.Guarderias.FindAsync(id);
            if (guarderia != null)
            {
                _context.Guarderias.Remove(guarderia);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Guardería eliminada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
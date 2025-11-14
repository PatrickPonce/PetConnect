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
    [Route("Admin/Mascotas")] // Nueva ruta
    public class MascotasAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MascotasAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Mascotas
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.Mascotas.OrderByDescending(p => p.FechaPublicacion).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Nombre.Contains(searchString) || p.Tipo.Contains(searchString) || p.Raza.Contains(searchString));
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedMascotas = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedMascotas);
        }

        // GET: Admin/Mascotas/Crear
        [Route("Crear")]
        public IActionResult Create()
        {
            return View(new Mascota { Tipo = "Perro", Genero = "Macho", Edad = "Adulto" });
        }

        // POST: Admin/Mascotas/Crear
        [HttpPost]
        [Route("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mascota mascota)
        {
            if (ModelState.IsValid)
            {
                mascota.FechaPublicacion = DateTime.UtcNow;
                _context.Add(mascota);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mascota agregada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            return View(mascota);
        }

        // GET: Admin/Mascotas/Editar/5
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // POST: Admin/Mascotas/Editar/5
        [HttpPost]
        [Route("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var mascotaToUpdate = await _context.Mascotas.FindAsync(id);
            if (mascotaToUpdate == null) return NotFound();

            if (await TryUpdateModelAsync<Mascota>(
                mascotaToUpdate,
                "", // Prefijo
                m => m.Nombre, m => m.Tipo, m => m.Raza, m => m.Edad, m => m.Genero,
                m => m.Descripcion, m => m.UrlImagen, m => m.Temperamento, m => m.Contacto))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Mascota actualizada con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. Fue modificado por otro usuario.");
                }
            }
            return View(mascotaToUpdate);
        }

        // GET: Admin/Mascotas/Eliminar/5
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id);
            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // POST: Admin/Mascotas/DeleteConfirmed/5
        [HttpPost]
        [Route("DeleteConfirmed/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota != null)
            {
                _context.Mascotas.Remove(mascota);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mascota eliminada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
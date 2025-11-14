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
    [Route("Admin/Lugares")] // Ruta limpia para el admin
    public class LugaresAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LugaresAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Lugares
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.LugaresPetFriendly.OrderBy(p => p.Nombre).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Nombre.Contains(searchString) || p.Categoria.Contains(searchString) || p.Ubicacion.Contains(searchString));
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedLugares = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedLugares);
        }

        // GET: Admin/Lugares/Crear
        [Route("Crear")]
        public IActionResult Create()
        {
            // Modelo vacío para el formulario
            return View(new LugarPetFriendly());
        }

        // POST: Admin/Lugares/Crear
        [HttpPost]
        [Route("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Categoria,Ubicacion,UrlImagenPrincipal,UrlLogo,DireccionCompleta,Calificacion,Telefono,UrlFacebook,UrlInstagram,Descripcion,Latitud,Longitud")] 
            LugarPetFriendly lugar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lugar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lugar Pet Friendly creado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            // Si falla, vuelve a mostrar el formulario con los datos
            return View(lugar);
        }

        // GET: Admin/Lugares/Editar/5
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lugar = await _context.LugaresPetFriendly.FindAsync(id);
            if (lugar == null) return NotFound();
            return View(lugar);
        }

        // POST: Admin/Lugares/Editar/5
        [HttpPost]
        [Route("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var lugarToUpdate = await _context.LugaresPetFriendly.FindAsync(id);
            if (lugarToUpdate == null) return NotFound();

            if (await TryUpdateModelAsync<LugarPetFriendly>(
                lugarToUpdate,
                "", // Prefijo vacío
                l => l.Nombre, l => l.Categoria, l => l.Ubicacion, l => l.UrlImagenPrincipal, 
                l => l.UrlLogo, l => l.DireccionCompleta, l => l.Calificacion, l => l.Telefono, 
                l => l.UrlFacebook, l => l.UrlInstagram, l => l.Descripcion, l => l.Latitud, l => l.Longitud))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lugar actualizado con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. El lugar fue modificado por otro usuario.");
                }
            }
            
            return View(lugarToUpdate);
        }

        // GET: Admin/Lugares/Eliminar/5
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var lugar = await _context.LugaresPetFriendly.FirstOrDefaultAsync(m => m.Id == id);
            if (lugar == null) return NotFound();
            return View(lugar);
        }

        // POST: Admin/Lugares/DeleteConfirmed/5
        [HttpPost]
        [Route("DeleteConfirmed/{id:int}")] // <-- CORRECCIÓN AQUÍ
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lugar = await _context.LugaresPetFriendly.FindAsync(id);
            if (lugar != null)
            {
                _context.LugaresPetFriendly.Remove(lugar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lugar eliminado con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
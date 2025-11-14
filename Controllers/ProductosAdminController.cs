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
    [Route("Admin/Productos")] // Una ruta limpia para el admin
    public class ProductosAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Productos
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.ProductosPetShop.OrderBy(p => p.Nombre).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Nombre.Contains(searchString) || p.TipoProducto.Contains(searchString));
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedProductos = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedProductos);
        }

        // GET: Admin/Productos/Crear
        [Route("Crear")]
        public IActionResult Create()
        {
            // Pasamos un modelo vacío con valores por defecto si es necesario
            var model = new ProductoPetShop
            {
                Nombre = "",
                Descripcion = "",
                TipoProducto = "Comida",
                QueryImagen = "pet food",
                Tags = new List<string>() // Inicializa la lista
            };
            // Usamos ViewData para el string de tags del formulario
            ViewData["TagsString"] = "";
            return View(model);
        }

        // POST: Admin/Productos/Crear
        [HttpPost]
        [Route("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,TipoProducto,QueryImagen")] ProductoPetShop producto, string tagsString)
        {
            if (ModelState.IsValid)
            {
                // Convertimos el string de tags en una List<string>
                if (!string.IsNullOrEmpty(tagsString))
                {
                    producto.Tags = tagsString.Split(',')
                                        .Select(tag => tag.Trim())
                                        .Where(tag => !string.IsNullOrEmpty(tag))
                                        .ToList();
                }
                else
                {
                    producto.Tags = new List<string>();
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto creado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            // Si falla, volvemos a poblar el string de tags
            ViewData["TagsString"] = tagsString;
            return View(producto);
        }

        // GET: Admin/Productos/Editar/5
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.ProductosPetShop.FindAsync(id);
            if (producto == null) return NotFound();

            // Convertimos la List<string> a un string simple para el formulario
            ViewData["TagsString"] = string.Join(", ", producto.Tags ?? new List<string>());
            return View(producto);
        }

        // POST: Admin/Productos/Editar/5
        [HttpPost]
        [Route("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string tagsString)
        {
            var productoToUpdate = await _context.ProductosPetShop.FindAsync(id);
            if (productoToUpdate == null) return NotFound();

            // Intentamos actualizar el modelo desde el formulario
            if (await TryUpdateModelAsync<ProductoPetShop>(
                productoToUpdate,
                "", // Prefijo vacío
                p => p.Nombre, p => p.Descripcion, p => p.Precio, p => p.TipoProducto, p => p.QueryImagen))
            {
                // Convertimos el string de tags en una List<string>
                if (!string.IsNullOrEmpty(tagsString))
                {
                    productoToUpdate.Tags = tagsString.Split(',')
                                        .Select(tag => tag.Trim())
                                        .Where(tag => !string.IsNullOrEmpty(tag))
                                        .ToList();
                }
                else
                {
                    productoToUpdate.Tags = new List<string>();
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. El producto fue modificado por otro usuario.");
                }
            }
            
            // Si falla, volvemos a poblar el string de tags
            ViewData["TagsString"] = tagsString;
            return View(productoToUpdate);
        }

        // GET: Admin/Productos/Eliminar/5
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.ProductosPetShop.FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // POST: Admin/Productos/Eliminar/5
        [HttpPost, ActionName("Delete")]
        [Route("Eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.ProductosPetShop.FindAsync(id);
            if (producto != null)
            {
                _context.ProductosPetShop.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto eliminado con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
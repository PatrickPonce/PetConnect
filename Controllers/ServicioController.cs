
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models; 
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Controllers
{
    public class ServicioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Muestra la lista de todas las veterinarias
        public async Task<IActionResult> Index(string busqueda)
        {
            IQueryable<Servicio> query = _context.Servicios
                .Where(s => s.Tipo == TipoServicio.Veterinaria)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(s => s.Nombre.Contains(busqueda));
                ViewData["BusquedaActual"] = busqueda;
            }

            var veterinarias = await query.ToListAsync();
            return View(veterinarias);
        }

        // Muestra el detalle de una veterinaria específica
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var veterinaria = await _context.Servicios
                .Include(s => s.VeterinariaDetalle) // Carga los detalles
                    .ThenInclude(vd => vd.Resenas) // Y luego carga las reseñas de esos detalles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.Veterinaria);

            if (veterinaria == null) return NotFound();

            return View(veterinaria);
        }
    }
}
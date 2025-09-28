using X.PagedList;
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
        public async Task<IActionResult> Index(string busqueda, int? page)
        {
            ViewData["BusquedaActual"] = busqueda;
            
            IQueryable<Servicio> query = _context.Servicios
                .Where(s => s.Tipo == TipoServicio.Veterinaria)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(s => s.Nombre.Contains(busqueda));
            }
            
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            var veterinariasPaginadas = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(veterinariasPaginadas);
        }   

        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var veterinaria = await _context.Servicios
                .Include(s => s.VeterinariaDetalle) 
                    .ThenInclude(vd => vd.Resenas) 
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.Veterinaria);

            if (veterinaria == null) return NotFound();

            return View(veterinaria);
        }
    }
}
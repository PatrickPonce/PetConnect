using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data; 
using PetConnect.Models; 
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Controllers 
{
    public class LugaresPetFriendlyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LugaresPetFriendlyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string busqueda)
        {
            var serviciosQuery = _context.Servicios
                                        .Include(s => s.Categoria);

        
            serviciosQuery = serviciosQuery.Where(s => s.Categoria != null && s.Categoria.Nombre == "Lugares Pet Friendly");
        

            if (!string.IsNullOrEmpty(busqueda))
            {
                serviciosQuery = serviciosQuery.Where(s => s.Nombre.ToLower().Contains(busqueda.ToLower()) || 
                                                        s.DescripcionCorta.ToLower().Contains(busqueda.ToLower()));
                ViewData["BusquedaActual"] = busqueda;
            }

            var lugaresPetFriendly = await serviciosQuery.ToListAsync();
            
            return View(lugaresPetFriendly);
        }
    }
}
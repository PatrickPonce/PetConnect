using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para .Include() y .ToListAsync()
using PetConnect.Data; // Reemplaza esto con el namespace de tu DbContext
using PetConnect.Models; // Reemplaza esto con el namespace de tus modelos
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Controllers // Asegúrate de que el namespace sea el correcto
{
    public class AdopcionController : Controller
    {
        private readonly ApplicationDbContext _context; // Asumiendo que tu DbContext se llama así

        public AdopcionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // La acción Index recibe el término de búsqueda opcional
        public async Task<IActionResult> Index(string busqueda)
        {
            // 1. Empezamos la consulta base, incluyendo la información de la Categoría
            IQueryable<Servicio> serviciosQuery = _context.Servicios
                                                          .Include(s => s.Categoria);

            // 2. FILTRO CLAVE: Seleccionamos solo los servicios cuya categoría se llame "Adopción de Mascotas"
            serviciosQuery = serviciosQuery.Where(s => s.Categoria != null && s.Categoria.Nombre == "Adopción de Mascotas");

            // 3. Si hay un término de búsqueda, aplicamos un filtro adicional
            if (!string.IsNullOrEmpty(busqueda))
            {
                // Buscamos en el nombre y descripción del servicio (insensible a mayúsculas)
                serviciosQuery = serviciosQuery.Where(s => s.Nombre.ToLower().Contains(busqueda.ToLower()) || 
                                                           s.DescripcionCorta.ToLower().Contains(busqueda.ToLower()));
                
                // Pasamos el término de búsqueda a la vista para feedback al usuario
                ViewData["BusquedaActual"] = busqueda;
            }

            // 4. Ejecutamos la consulta y la pasamos a la vista
            var serviciosDeAdopcion = await serviciosQuery.ToListAsync();
            
            return View(serviciosDeAdopcion);
        }
    }
}
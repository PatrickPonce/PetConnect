// Controllers/AdopcionController.cs
using Microsoft.AspNetCore.Mvc;
using PetConnect.Services; // Importa tu servicio
using System.Threading.Tasks;
using System.Linq;

namespace PetConnect.Controllers
{
    public class AdopcionController : Controller
    {
        private readonly AnimalApiService _animalApiService;

        public AdopcionController(AnimalApiService animalApiService)
        {
            _animalApiService = animalApiService;
        }

        public async Task<IActionResult> Index(string busqueda)
        {
            // Obtenemos todos los animales de la API
            var todosLosAnimales = await _animalApiService.ObtenerAnimalesAdopcionAsync();

            var animalesFiltrados = todosLosAnimales;
            
            // Si hay un término de búsqueda, filtramos la lista en memoria
            if (!string.IsNullOrEmpty(busqueda))
            {
                string busquedaLower = busqueda.ToLower();
                animalesFiltrados = todosLosAnimales
                    .Where(a => a.Nombre.ToLower().Contains(busquedaLower) || 
                                a.Temperamento.ToLower().Contains(busquedaLower) ||
                                a.Tipo.ToLower().Contains(busquedaLower))
                    .ToList();
            }

            ViewData["BusquedaActual"] = busqueda;
            return View(animalesFiltrados);
        }
    }
}
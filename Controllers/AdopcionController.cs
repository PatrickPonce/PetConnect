// PetConnect/Controllers/AdopcionController.cs

using Microsoft.AspNetCore.Mvc;
using PetConnect.Services; // Para AnimalApiService
using PetConnect.Data; // Para ApplicationDbContext
using PetConnect.Models.Api; // Para AnimalViewModel
using PetConnect.Models; // Para Mascota
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Para List<T>
using Microsoft.EntityFrameworkCore; // Para ToListAsync

namespace PetConnect.Controllers
{
    public class AdopcionController : Controller
    {
        private readonly AnimalApiService _animalApiService;
        private readonly ApplicationDbContext _context; // <-- 1. Añade el DbContext

        // 2. Modifica el constructor para inyectar ApplicationDbContext
        public AdopcionController(AnimalApiService animalApiService, ApplicationDbContext context)
        {
            _animalApiService = animalApiService;
            _context = context;
        }

        // 3. Reemplaza tu método Index con este
        public async Task<IActionResult> Index(string busqueda)
        {
            // --- TAREA 1: OBTENER MASCOTAS DE LA API (Como antes) ---
            var apiAnimales = await _animalApiService.ObtenerAnimalesAdopcionAsync();

            // --- TAREA 2: OBTENER MASCOTAS DE LA BD LOCAL ---
            var mascotasLocales = await _context.Mascotas.AsNoTracking().ToListAsync();

            // --- TAREA 3: CONVERTIR MASCOTAS LOCALES AL VIEWMODEL COMÚN ---
            // (Usamos el mismo AnimalViewModel que usa la API para que las tarjetas sean iguales)
            var animalesLocalesVm = mascotasLocales.Select(m => new AnimalViewModel
            {
                Id = $"local-{m.Id}", // Damos un ID único
                Nombre = m.Nombre,
                UrlImagen = m.UrlImagen ?? "/images/placeholder.png",
                Origen = m.Contacto ?? "Refugio Local", // Usamos el campo Contacto como Origen
                Temperamento = m.Temperamento ?? "No especificado",
                Tipo = m.Tipo
            }).ToList();

            // --- TAREA 4: COMBINAR LAS DOS LISTAS ---
            var todosLosAnimales = new List<AnimalViewModel>();
            todosLosAnimales.AddRange(apiAnimales);
            todosLosAnimales.AddRange(animalesLocalesVm);

            // Mezclamos la lista para que las mascotas locales y de la API aparezcan intercaladas
            var animalesMezclados = todosLosAnimales.OrderBy(a => Guid.NewGuid()).ToList();
            
            // --- TAREA 5: APLICAR EL FILTRO (Igual que antes) ---
            var animalesFiltrados = animalesMezclados;
            
            if (!string.IsNullOrEmpty(busqueda))
            {
                string busquedaLower = busqueda.ToLower();
                animalesFiltrados = animalesMezclados
                    .Where(a => (a.Nombre != null && a.Nombre.ToLower().Contains(busquedaLower)) || 
                                (a.Temperamento != null && a.Temperamento.ToLower().Contains(busquedaLower)) ||
                                (a.Tipo != null && a.Tipo.ToLower().Contains(busquedaLower)))
                    .ToList();
            }

            ViewData["BusquedaActual"] = busqueda;
            return View(animalesFiltrados); // Enviamos la lista combinada y filtrada
        }
    }
}
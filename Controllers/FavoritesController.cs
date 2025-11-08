using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Services;
using PetConnect.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PexelsService _pexelsService;

    public FavoritesController(ApplicationDbContext context, PexelsService pexelsService)
    {
        _context = context;
        _pexelsService = pexelsService;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Lugares
        var lugaresQuery = _context.FavoritosLugar
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.LugarPetFriendly);

        // 2. Guarderías
        var guarderiasQuery = _context.FavoritosGuarderia
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.Guarderia);

        // 3. Noticias
        var noticiasQuery = _context.Noticias // Asumiendo que tienes una tabla FavoritosNoticia...
             // .Where(f => f.UsuarioId == userId) // ...necesitas implementar la lógica de favoritos de noticias
             .Select(f => f); // TEMPORAL: Muestra todas las noticias. Ajusta esto.

        // 4. --- LÍNEA FALTANTE ---
        // (Asumiendo que ya creaste FavoritoProducto y ProductoPetShop como te indiqué)
        var productosQuery = _context.FavoritosProducto
            .Where(f => f.UsuarioId == userId)
            .Include(f => f.ProductoPetShop) // ¡Importante incluir esto!
            .Select(f => f.ProductoPetShop);
        // --- FIN ---
        
        var productosFavoritos = await productosQuery.AsNoTracking().ToListAsync();

        // 2. Hacemos un bucle y llamamos a Pexels (API 2) para cada producto
        foreach (var producto in productosFavoritos)
        {
            producto.UrlImagen = await _pexelsService.ObtenerImagenAsync(producto.QueryImagen);
        }

        // 5. Aplicar el filtro de búsqueda

        // --- INICIO DE LA INTEGRACIÓN DE SERVICIOS ---

        // 4. AÑADIR: Obtener los Servicios (Veterinarias) Favoritos
        var serviciosQuery = _context.FavoritosServicio
            .Where(f => f.UsuarioId == userId)
            .Where(f => f.UsuarioId == userId)
            .Include(f => f.Servicio) // Incluimos el Servicio
                .ThenInclude(s => s.VeterinariaDetalle) // Incluimos los detalles si los necesitamos para el filtro
            .Select(f => f.Servicio);
        

        // 3. Aplicar el filtro de búsqueda a AMBAS listas
        if (!string.IsNullOrEmpty(searchString))
        {
            lugaresQuery = lugaresQuery.Where(l => l.Nombre.Contains(searchString) || l.Ubicacion.Contains(searchString));
            guarderiasQuery = guarderiasQuery.Where(g => g.Nombre.Contains(searchString) || g.Ubicacion.Contains(searchString));
            noticiasQuery = noticiasQuery.Where(n => n.Titulo.Contains(searchString) || n.Contenido.Contains(searchString)); // <-- Filtro de noticias corregido
            productosQuery = productosQuery.Where(p => p.Nombre.Contains(searchString)); // <-- Filtro de productos añadido
            noticiasQuery = noticiasQuery.Where(n => n.Titulo.Contains(searchString) || n.Contenido.Contains(searchString));
            serviciosQuery = serviciosQuery.Where(s => s.Nombre.Contains(searchString) || (s.VeterinariaDetalle != null && s.VeterinariaDetalle.Direccion.Contains(searchString)));
        }

        // 6. Crear el ViewModel
        var viewModel = new FavoritosViewModel
        {
            LugaresFavoritos = await lugaresQuery.AsNoTracking().ToListAsync(),
            GuarderiasFavoritas = await guarderiasQuery.AsNoTracking().ToListAsync(),
            NoticiasFavoritas = await noticiasQuery.AsNoTracking().ToListAsync(),
            ProductosFavoritos = productosFavoritos, // <-- Línea corregida
            ServiciosFavoritos = await serviciosQuery.AsNoTracking().ToListAsync()
        };

        return View(viewModel);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PetConnect.ViewModels; // Asegúrate de que tu ProductoViewModel esté aquí

public class PetShopController : Controller
{
    private readonly ApplicationDbContext _context; 
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PexelsService _pexelsService;
    private readonly YouTubeService _youTubeService;

    public PetShopController(ApplicationDbContext context, UserManager<IdentityUser> userManager, PexelsService pexelsService, YouTubeService youTubeService)
    {
        _context = context;
        _userManager = userManager;
        _pexelsService = pexelsService;
        _youTubeService = youTubeService;
    }

    // --- ACCIÓN INDEX (MENÚ DE CATEGORÍAS) ---
    public IActionResult Index()
    {
        return View();
    }

    // --- ACCIÓN DE GALERÍA (CON ÁMBITO CORREGIDO) ---
public async Task<IActionResult> Productos(string tipo, string busqueda, string tag)
{
    var productosQuery = _context.ProductosPetShop.AsQueryable();

    // --- Lógica de Filtros ---
    if (!string.IsNullOrEmpty(busqueda))
    {
        var busquedaLower = busqueda.ToLower();
        productosQuery = productosQuery.Where(p => 
            p.Nombre.ToLower().Contains(busquedaLower)
        );
    }
    if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
    {
        productosQuery = productosQuery.Where(p => p.TipoProducto.ToLower() == tipo.ToLower());
    }
    if (!string.IsNullOrEmpty(tag))
    {
        productosQuery = productosQuery.Where(p => p.Tags.Contains(tag));
    }
    // --- Fin Lógica de Filtros ---

    var productos = await productosQuery.ToListAsync(); 

    // =======================================================
    // INICIO DE LA CORRECCIÓN
    // 1. Declaramos la variable 'favoritosIds' aquí fuera
    // =======================================================
    HashSet<int> favoritosIds = new HashSet<int>();

    if (User.Identity.IsAuthenticated)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // 2. Llenamos la variable (ya no la declaramos con 'var')
        favoritosIds = await _context.FavoritosProducto
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.ProductoPetShopId)
            .ToHashSetAsync();
    }
    // =======================================================
    // FIN DE LA CORRECCIÓN
    // =======================================================

    // 3. Crear ViewModels y obtener imágenes (API 2: Pexels)
    var productosVM = new List<ProductoViewModel>(); // <-- Esta línea ya no dará error
    foreach (var prod in productos)
    {
        prod.UrlImagen = await _pexelsService.ObtenerImagenAsync(prod.QueryImagen);

        productosVM.Add(new ProductoViewModel { // <-- Esta tampoco
            Producto = prod, 
            EsFavorito = favoritosIds.Contains(prod.Id) // <-- Y esta tampoco
        });
    }

    // 4. Pasar filtros a la vista
    ViewData["TiposProducto"] = (await _context.ProductosPetShop.Select(p => p.TipoProducto).Distinct().ToListAsync());
    ViewData["BusquedaActual"] = busqueda;
    ViewData["TipoActual"] = tipo ?? "Todos";

    return View("Productos", productosVM);
    }

    // 3. ACTUALIZA LA ACCIÓN 'DETALLE'
    public async Task<IActionResult> Detalle(int id)
    {
        var producto = await _context.ProductosPetShop.FindAsync(id); 
        if (producto == null) return NotFound();
        
        // API 2: Obtener imagen de Pexels (como antes)
        producto.UrlImagen = await _pexelsService.ObtenerImagenAsync(producto.QueryImagen);
        
        // --- INICIO: NUEVA LÓGICA DE YOUTUBE ---
        // API 3: Obtener videos de YouTube
        var videos = await _youTubeService.BuscarVideosAsync(producto.Nombre);
        ViewBag.VideoIds = videos; // Pasamos la lista de IDs a la vista
        // --- FIN: NUEVA LÓGICA DE YOUTUBE ---
        
        return View(producto);
    }

    // --- ACCIÓN FAVORITOS (Sin cambios) ---
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorito(int productoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var favoritoExistente = await _context.FavoritosProducto
            .FirstOrDefaultAsync(f => f.ProductoPetShopId == productoId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosProducto.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            _context.FavoritosProducto.Add(new FavoritoProducto { ProductoPetShopId = productoId, UsuarioId = userId });
            agregado = true;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, agregado = agregado });
    }
}
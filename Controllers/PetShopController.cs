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
using PetConnect.ViewModels;

public class PetShopController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PexelsService _pexelsService;
    private readonly YouTubeService _youTubeService;
    private readonly GoogleSearchService _googleSearchService; // Servicio añadido

    public PetShopController(
        ApplicationDbContext context, 
        UserManager<IdentityUser> userManager, 
        PexelsService pexelsService, 
        YouTubeService youTubeService, 
        GoogleSearchService googleSearchService) // Inyección añadida
    {
        _context = context;
        _userManager = userManager;
        _pexelsService = pexelsService;
        _youTubeService = youTubeService;
        _googleSearchService = googleSearchService; // Asignación añadida
    }

    // --- ACCIÓN INDEX (MENÚ DE CATEGORÍAS) ---
    public IActionResult Index()
    {
        return View();
    }

    // --- ACCIÓN DE GALERÍA (PRODUCTOS) ---
    public async Task<IActionResult> Productos(string tipo, string busqueda, string tag, string marca) // Parámetro 'marca' añadido
    {
        // ============================================================
        // NUEVA LÓGICA: SI HAY MARCA, USAMOS GOOGLE Y TERMINAMOS
        // ============================================================
        if (!string.IsNullOrEmpty(marca))
        {
            ViewData["Title"] = $"Marca: {marca}";
            ViewData["MarcaActual"] = marca; // Para resaltar en el menú lateral

            // Llamamos a Google y obtenemos resultados externos
            var productosExternos = await _googleSearchService.BuscarProductosExternos(marca);
            
            // Pasamos datos vacíos para los filtros para evitar errores en la vista
            ViewData["TiposProducto"] = new List<string>();
            ViewData["TipoActual"] = "Todos";

            return View("Productos", productosExternos);
        }
        // ============================================================

        var productosQuery = _context.ProductosPetShop.AsQueryable();

        // --- Filtros (BD Local) ---
        if (!string.IsNullOrEmpty(busqueda))
        {
            productosQuery = productosQuery.Where(p => p.Nombre.ToLower().Contains(busqueda.ToLower()));
        }
        if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
        {
            productosQuery = productosQuery.Where(p => p.TipoProducto.ToLower() == tipo.ToLower());
        }
        if (!string.IsNullOrEmpty(tag))
        {
            productosQuery = productosQuery.Where(p => p.Tags.Contains(tag));
        }

        var productos = await productosQuery.ToListAsync();

        // --- Favoritos (Solo para productos locales) ---
        HashSet<int> favoritosIds = new HashSet<int>();
        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            favoritosIds = await _context.FavoritosProducto
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.ProductoPetShopId)
                .ToHashSetAsync();
        }
            
        // --- ViewModels + Imágenes Pexels (CORREGIDO) ---
        var productosVM = new List<ProductoViewModel>();
        foreach (var prod in productos)
        {
            // 1. Obtener imagen de Pexels
            string imagenPexels = await _pexelsService.ObtenerImagenAsync(prod.QueryImagen);
            
            // 2. Llenar el ViewModel PLANO
            productosVM.Add(new ProductoViewModel { 
                // Propiedades comunes (necesarias para la vista unificada)
                Id = prod.Id,
                Nombre = prod.Nombre,
                Descripcion = prod.Descripcion,
                Precio = prod.Precio,
                UrlImagen = imagenPexels,
                Tags = prod.Tags,
                
                // Propiedades específicas
                EsExterno = false, // Es un producto interno
                EsFavorito = favoritosIds.Contains(prod.Id),
                Producto = prod // Mantenemos la referencia por si acaso
            });
        }

        // --- Datos para la Vista ---
        ViewData["TiposProducto"] = (await _context.ProductosPetShop.Select(p => p.TipoProducto).Distinct().ToListAsync());
        ViewData["BusquedaActual"] = busqueda;
        ViewData["TipoActual"] = tipo ?? "Todos";
        ViewData["TagActual"] = tag; 

        return View("Productos", productosVM);
    }

    // --- ACCIÓN DETALLE (PRODUCTOS LOCALES) ---
    public async Task<IActionResult> Detalle(int id)
    {
        var producto = await _context.ProductosPetShop.FindAsync(id); 
        if (producto == null) return NotFound();
        
        producto.UrlImagen = await _pexelsService.ObtenerImagenAsync(producto.QueryImagen);
        
        // Videos de YouTube
        var videos = await _youTubeService.BuscarVideosAsync(producto.Nombre);
        ViewBag.VideoIds = videos;
        
        return View(producto);
    }

    // --- NUEVA ACCIÓN: DETALLE PARA PRODUCTOS EXTERNOS (GOOGLE) ---
    public IActionResult DetalleExterno(string nombre, string imagen, string descripcion, string url, string tienda)
    {
        var modelo = new ProductoViewModel
        {
            Nombre = nombre,
            UrlImagen = imagen,
            Descripcion = descripcion,
            UrlExterna = url,
            NombreTienda = tienda,
            EsExterno = true,
            Precio = 0 
        };
        return View(modelo);
    }

    // --- ACCIÓN FAVORITOS (SOLO LOCALES) ---
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
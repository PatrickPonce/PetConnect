using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;

    public FavoritesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Obtener los Lugares Favoritos (esto ya lo tenías y funciona)
        var lugaresQuery = _context.FavoritosLugar
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.LugarPetFriendly);

        // 2. AÑADIR: Obtener las Guarderías Favoritas (esta es la parte que falta)
        var guarderiasQuery = _context.FavoritosGuarderia
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.Guarderia);
        var noticiasQuery = _context.Favoritos
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.Noticia);


        // --- INICIO DE LA INTEGRACIÓN DE SERVICIOS ---

        // 4. AÑADIR: Obtener los Servicios (Veterinarias) Favoritos
        var serviciosQuery = _context.FavoritosServicio
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.Servicio);
        

        // 3. Aplicar el filtro de búsqueda a AMBAS listas
        if (!string.IsNullOrEmpty(searchString))
        {
            lugaresQuery = lugaresQuery.Where(l => l.Nombre.Contains(searchString) || l.Ubicacion.Contains(searchString));
            guarderiasQuery = guarderiasQuery.Where(g => g.Nombre.Contains(searchString) || g.Ubicacion.Contains(searchString));
            noticiasQuery = noticiasQuery.Where(n => n.Titulo.Contains(searchString) || n.Contenido.Contains(searchString));
            serviciosQuery = serviciosQuery.Where(s => s.Nombre.Contains(searchString) || (s.VeterinariaDetalle != null && s.VeterinariaDetalle.Direccion.Contains(searchString)));
        }

        // 4. Crear el ViewModel y poblar AMBAS listas
        var viewModel = new FavoritosViewModel
        {
            LugaresFavoritos = await lugaresQuery.AsNoTracking().ToListAsync(),
            GuarderiasFavoritas = await guarderiasQuery.AsNoTracking().ToListAsync(),
            NoticiasFavoritas = await noticiasQuery.AsNoTracking().ToListAsync(),
            ServiciosFavoritos = await serviciosQuery.AsNoTracking().ToListAsync()
        };

        return View(viewModel);
    }
}
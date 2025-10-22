using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; 
using PetConnect.Data; 
using PetConnect.Models; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 
using System.Collections.Generic;

public class NoticiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    // --- Añadido ---
    private readonly SignInManager<IdentityUser> _signInManager; 

    public NoticiasController(ApplicationDbContext context, 
                                UserManager<IdentityUser> userManager, 
                                SignInManager<IdentityUser> signInManager) // Añadido signInManager
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager; 
    }

 
    public async Task<IActionResult> Index()
    {
        var noticias = await _context.Noticias
                                      .OrderByDescending(n => n.FechaPublicacion)
                                      .ToListAsync();

        var favoritosDelUsuario = new HashSet<int>();
        if (_signInManager.IsSignedIn(User))
        {
            var userId = _userManager.GetUserId(User);
      
            var ids = await _context.Favoritos
                                    .Where(f => f.UsuarioId == userId)
                                    .Select(f => f.NoticiaId)
                                    .ToListAsync();
            favoritosDelUsuario = new HashSet<int>(ids);
        }

        ViewData["FavoritosDelUsuario"] = favoritosDelUsuario;

        foreach (var noticia in noticias)
        {
            string contenidoLimpio = Regex.Replace(noticia.Contenido ?? string.Empty, "<.*?>", String.Empty);
            
            if (contenidoLimpio.Length > 100)
            {
                noticia.Contenido = contenidoLimpio.Substring(0, 100) + "...";
            } 
            else
            {
                noticia.Contenido = contenidoLimpio;
            }
        }

        return View(noticias);
    }
    
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var noticia = await _context.Noticias
                                    .Include(n => n.Comentarios) 
                                    .FirstOrDefaultAsync(n => n.Id == id);

        if (noticia == null)
        {
            return NotFound();
        }

        bool esFavorito = false;
        if (_signInManager.IsSignedIn(User))
        {
            var userId = _userManager.GetUserId(User);
            esFavorito = await _context.Favoritos
                .AnyAsync(f => f.NoticiaId == noticia.Id && f.UsuarioId == userId);
        }

        ViewData["EsFavorito"] = esFavorito; 
        Console.WriteLine($"DEBUG CONTROLLER Detalle: User.Identity.IsAuthenticated = {User.Identity.IsAuthenticated}");

        return View(noticia);
    }

    [Authorize] 
    public async Task<IActionResult> Favoritos()
    {
        var userId = _userManager.GetUserId(User);

       

        var noticiasFavoritas = await _context.Favoritos
            .Where(f => f.UsuarioId == userId)  
            .OrderByDescending(f => f.FechaAgregado) 
            .Select(f => f.Noticia) 
            .ToListAsync();
        
        var favoritosDelUsuario = new HashSet<int>(noticiasFavoritas.Select(n => n.Id));
        ViewData["FavoritosDelUsuario"] = favoritosDelUsuario;
      
        foreach (var noticia in noticiasFavoritas)
        {
             string contenidoLimpio = Regex.Replace(noticia.Contenido ?? string.Empty, "<.*?>", String.Empty);
             if (contenidoLimpio.Length > 100) { noticia.Contenido = contenidoLimpio.Substring(0, 100) + "..."; } 
             else { noticia.Contenido = contenidoLimpio; }
        }
        
        return View(noticiasFavoritas);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorito([FromBody] int noticiaId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        var favoritoExistente = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.NoticiaId == noticiaId && f.UsuarioId == userId);

        bool esFavoritoAhora;

        if (favoritoExistente != null)
        {
            _context.Favoritos.Remove(favoritoExistente);
            esFavoritoAhora = false;
        }
        else
        {
            var nuevoFavorito = new Favorito
            {
                NoticiaId = noticiaId,
                UsuarioId = userId,
                FechaAgregado = DateTime.UtcNow
            };
            _context.Favoritos.Add(nuevoFavorito);
            esFavoritoAhora = true;
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, esFavorito = esFavoritoAhora });
    }
    
  [HttpPost]
    [Authorize]
    public async Task<IActionResult> EliminarFavoritos([FromBody] List<int> noticiaIds)
    {
        if (noticiaIds == null || !noticiaIds.Any())
        {
            return Json(new { success = false, message = "No se seleccionaron noticias." });
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            // Aunque [Authorize] debería prevenir esto, es una buena práctica.
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        // --- CAMBIO PRINCIPAL: Buscar solo los que EXISTEN ---
        var favoritosAEliminar = await _context.Favoritos
            .Where(f => f.UsuarioId == userId && noticiaIds.Contains(f.NoticiaId))
            .ToListAsync();

        // Si encontramos algunos (o ninguno), procedemos a intentar eliminarlos
        if (favoritosAEliminar.Any())
        {
            _context.Favoritos.RemoveRange(favoritosAEliminar);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // --- MANEJO DEL ERROR ---
                // Si ocurre el error de concurrencia (0 filas afectadas), lo ignoramos
                // porque significa que los registros ya fueron eliminados por otra acción.
                Console.WriteLine($"Advertencia de concurrencia al eliminar favoritos: {ex.Message}"); 
                // Opcional: Loggear la advertencia en lugar de solo imprimirla
                // Consideramos esto como un éxito parcial o total, ya que el estado final es el deseado (registros eliminados).
            }
            catch (Exception ex) // Capturar otros posibles errores de BD
            {
                Console.WriteLine($"Error al guardar cambios al eliminar favoritos: {ex.Message}");
                // Devolver un error al cliente en caso de otros problemas
                return Json(new { success = false, message = "Ocurrió un error inesperado al eliminar." });
            }
        }
        // Siempre devolvemos éxito, ya que el objetivo (que no estén) se cumple.
        return Json(new { success = true });
    }
}
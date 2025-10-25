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
    private readonly UserManager<IdentityUser> _userManager; // <-- Vuelve a IdentityUser
    private readonly SignInManager<IdentityUser> _signInManager; // <-- Vuelve a IdentityUser

    public NoticiasController(ApplicationDbContext context, 
                              UserManager<IdentityUser> userManager, // <-- Vuelve a IdentityUser
                              SignInManager<IdentityUser> signInManager) // <-- Vuelve a IdentityUser
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
            
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        
        var favoritosAEliminar = await _context.Favoritos
            .Where(f => f.UsuarioId == userId && noticiaIds.Contains(f.NoticiaId))
            .ToListAsync();

        
        if (favoritosAEliminar.Any())
        {
            _context.Favoritos.RemoveRange(favoritosAEliminar);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
            
                Console.WriteLine($"Advertencia de concurrencia al eliminar favoritos: {ex.Message}");
                
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error al guardar cambios al eliminar favoritos: {ex.Message}");
               
                return Json(new { success = false, message = "Ocurrió un error inesperado al eliminar." });
            }
        }
        return Json(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int noticiaId, string textoComentario)
    {
        // ... (validaciones) ...

        var userId = _userManager.GetUserId(User); // Obtiene el ID del usuario actual
        var user = await _userManager.FindByIdAsync(userId); // Obtiene el IdentityUser

        // --- INICIO NUEVA LÓGICA PARA OBTENER NOMBRE ---
        string autorNombre = "Usuario Anónimo"; // Valor por defecto

        if (user != null)
        {
            // Intenta obtener el Claim "NombreCompleto" (o como lo hayas llamado al guardar)
            var nombreClaim = await _userManager.GetClaimsAsync(user);
            var nombreCompletoClaim = nombreClaim.FirstOrDefault(c => c.Type == "NombreCompleto"); // <-- REVISA ESTE NOMBRE

            if (nombreCompletoClaim != null && !string.IsNullOrEmpty(nombreCompletoClaim.Value))
            {
                autorNombre = nombreCompletoClaim.Value; // Usa el valor del Claim
            }
            else
            {
                // Si no hay Claim "NombreCompleto", usa el UserName (email) y córtalo
                autorNombre = user.UserName ?? "Usuario Anónimo";
                if (autorNombre.Contains("@"))
                {
                    autorNombre = autorNombre.Split('@')[0];
                }
            }
        }
        // --- FIN NUEVA LÓGICA ---

        var nuevoComentario = new Comentario
        {
            NoticiaId = noticiaId,
            Texto = textoComentario,
            Autor = autorNombre, // <-- Usa la nueva variable
            FechaComentario = DateTime.UtcNow
        };

        _context.Comentarios.Add(nuevoComentario);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            autor = nuevoComentario.Autor, // <-- Envía el nombre correcto
            texto = nuevoComentario.Texto,
            fechaISO = nuevoComentario.FechaComentario.ToString("o")
        });
    }
    [HttpPost]
    [Authorize] // Solo usuarios logueados pueden intentar borrar
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarComentario(int comentarioId)
    {
        if (comentarioId <= 0)
        {
            return Json(new { success = false, message = "ID de comentario inválido." });
        }

        var userId = _userManager.GetUserId(User);
        var comentario = await _context.Comentarios.FindAsync(comentarioId);

        if (comentario == null)
        {
            return Json(new { success = false, message = "Comentario no encontrado." });
        }

        // --- Verificación de Autor ---
        // Obtiene el UserName cortado del usuario actual
        var currentUser = User.Identity?.Name ?? string.Empty;
        if (currentUser.Contains("@"))
        {
            currentUser = currentUser.Split('@')[0];
        }

        // Compara con el autor guardado en el comentario
        if (comentario.Autor != currentUser)
        {
            // Si no es el autor, no permite borrar (a menos que seas admin, lógica no incluida aquí)
            return Json(new { success = false, message = "No tienes permiso para eliminar este comentario." });
        }
        // --- Fin Verificación ---

        _context.Comentarios.Remove(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true }); // Devuelve éxito
    }
}
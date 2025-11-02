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
    private readonly SignInManager<IdentityUser> _signInManager; 

    public NoticiasController(ApplicationDbContext context, 
                              UserManager<IdentityUser> userManager, 
                              SignInManager<IdentityUser> signInManager) 
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

        var userId = _userManager.GetUserId(User); 
        var user = await _userManager.FindByIdAsync(userId); 

        
        string autorNombre = "Usuario Anónimo";

        if (user != null)
        {
            
            var nombreClaim = await _userManager.GetClaimsAsync(user);
            var nombreCompletoClaim = nombreClaim.FirstOrDefault(c => c.Type == "NombreCompleto"); // <-- REVISA ESTE NOMBRE

            if (nombreCompletoClaim != null && !string.IsNullOrEmpty(nombreCompletoClaim.Value))
            {
                autorNombre = nombreCompletoClaim.Value; 
            }
            else
            {
                
                autorNombre = user.UserName ?? "Usuario Anónimo";
                if (autorNombre.Contains("@"))
                {
                    autorNombre = autorNombre.Split('@')[0];
                }
            }
        }
        
        var nuevoComentario = new Comentario
        {
            NoticiaId = noticiaId,
            Texto = textoComentario,
            Autor = autorNombre, 
            FechaComentario = DateTime.UtcNow
        };

        _context.Comentarios.Add(nuevoComentario);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            autor = nuevoComentario.Autor, 
            texto = nuevoComentario.Texto,
            fechaISO = nuevoComentario.FechaComentario.ToString("o")
        });
    }
    [HttpPost]
    [Authorize] 
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

    
        var currentUser = User.Identity?.Name ?? string.Empty;
        if (currentUser.Contains("@"))
        {
            currentUser = currentUser.Split('@')[0];
        }

        
        if (comentario.Autor != currentUser)
        {
            
            return Json(new { success = false, message = "No tienes permiso para eliminar este comentario." });
        }
       

        _context.Comentarios.Remove(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true }); 
    }
   [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarComentario(int comentarioId, string nuevoTexto) // <-- ¡SIN [FromForm]!
    {
       
        if (comentarioId <= 0 || string.IsNullOrWhiteSpace(nuevoTexto) || nuevoTexto.Length < 3 || nuevoTexto.Length > 500)
        {
            return Json(new { success = false, message = "El comentario no es válido (3-500 caracteres)." });
        }

        var userId = _userManager.GetUserId(User);
        var comentario = await _context.Comentarios.FindAsync(comentarioId);

        if (comentario == null)
        {
            return Json(new { success = false, message = "Comentario no encontrado." });
        }

        var currentUser = User.Identity?.Name ?? string.Empty;
        if (currentUser.Contains("@")) { currentUser = currentUser.Split('@')[0]; }

        if (comentario.Autor != currentUser)
        {
            return Json(new { success = false, message = "No tienes permiso para editar este comentario." });
        }

       
        TimeSpan diferencia = DateTime.UtcNow - comentario.FechaComentario.ToUniversalTime();
        if (diferencia.TotalMinutes > 15)
        {
            return Json(new { success = false, message = "Ya no puedes editar este comentario (límite de 15 min)." });
        }


        comentario.Texto = nuevoTexto; 
        comentario.FechaComentario = DateTime.UtcNow;
        _context.Comentarios.Update(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true, texto = nuevoTexto,fechaISO = comentario.FechaComentario.ToString("o") });
    }
}
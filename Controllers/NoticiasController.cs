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
using System.Security.Claims;
using PetConnect.Claims;

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
    public async Task<IActionResult> ToggleFavorito([FromForm] int noticiaId)
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
    public async Task<IActionResult> EliminarFavoritos([FromForm] List<int> noticiaIds)
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
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Usuario no encontrado." });
        }

        // 1. Obtener la Foto de Perfil (Usando la lógica de Claims)
        var claims = await _userManager.GetClaimsAsync(user);
        var fotoClaim = claims.FirstOrDefault(c => c.Type == PetConnectClaimTypes.ProfilePictureUrl);
        var fotoUrl = fotoClaim?.Value ?? "/images/avatars/default.png"; // <-- Tu ruta por defecto

        // 2. Obtener el Nombre de Usuario
        var autorNombre = user.UserName ?? "Anónimo";
        if (autorNombre.Contains("@"))
        {
            autorNombre = autorNombre.Split('@')[0];
        }
        
        // 3. Validar texto
        if (string.IsNullOrWhiteSpace(textoComentario) || textoComentario.Length < 3 || textoComentario.Length > 500)
        {
            return Json(new { success = false, message = "El comentario debe tener entre 3 y 500 caracteres." });
        }

        var nuevoComentario = new Comentario
        {
            NoticiaId = noticiaId,
            Texto = textoComentario,
            Autor = autorNombre, 
            FechaComentario = DateTime.UtcNow,
            
            // --- GUARDAR LOS DATOS NUEVOS ---
            AutorId = user.Id,         // <-- Guardar el ID
            AutorFotoUrl = fotoUrl     // <-- Guardar la Foto URL
        };

        _context.Comentarios.Add(nuevoComentario);
        await _context.SaveChangesAsync();

        // --- DEVOLVER LA FOTO URL EN EL JSON ---
        return Json(new
        {
            success = true,
            id = nuevoComentario.Id, // <-- Devolver el ID es buena idea
            autor = nuevoComentario.Autor, 
            texto = nuevoComentario.Texto,
            fechaISO = nuevoComentario.FechaComentario.ToString("o"),
            fotoUrl = nuevoComentario.AutorFotoUrl // <-- ¡El JS lo necesita!
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

        var userId = _userManager.GetUserId(User); // <-- Obtener el ID del usuario actual
        var comentario = await _context.Comentarios.FindAsync(comentarioId);

        if (comentario == null)
        {
            return Json(new { success = false, message = "Comentario no encontrado." });
        }

        // --- COMPROBACIÓN DE PERMISO CORRECTA ---
        if (comentario.AutorId != userId)
        {
            // El usuario actual no es el autor del comentario
            return Json(new { success = false, message = "No tienes permiso para eliminar este comentario." });
        }
        
        _context.Comentarios.Remove(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true }); 
    }
   [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarComentario(int comentarioId, string nuevoTexto)
    {
        if (comentarioId <= 0 || string.IsNullOrWhiteSpace(nuevoTexto) || nuevoTexto.Length < 3 || nuevoTexto.Length > 500)
        {
            return Json(new { success = false, message = "El comentario no es válido (3-500 caracteres)." });
        }

        var userId = _userManager.GetUserId(User); // <-- Obtener el ID del usuario actual
        var comentario = await _context.Comentarios.FindAsync(comentarioId);

        if (comentario == null)
        {
            return Json(new { success = false, message = "Comentario no encontrado." });
        }

        // --- COMPROBACIÓN DE PERMISO CORRECTA ---
        if (comentario.AutorId != userId)
        {
            return Json(new { success = false, message = "No tienes permiso para editar este comentario." });
        }

        TimeSpan diferencia = DateTime.UtcNow - comentario.FechaComentario.ToUniversalTime();
        if (diferencia.TotalMinutes > 15)
        {
            return Json(new { success = false, message = "Ya no puedes editar este comentario (límite de 15 min)." });
        }

        comentario.Texto = nuevoTexto; 
        comentario.FechaComentario = DateTime.UtcNow; // Actualizar la fecha al editar
        
        _context.Comentarios.Update(comentario);
        await _context.SaveChangesAsync();

        // --- JSON DE RESPUESTA CORREGIDO (SIN EL ERROR) ---
        return Json(new { 
            success = true, 
            texto = nuevoTexto, 
            fechaISO = comentario.FechaComentario.ToString("o") 
        });
    }
}
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
using PetConnect.Services;
using Microsoft.AspNetCore.SignalR;
using PetConnect.Hubs;
public class NoticiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly PerspectiveService _perspectiveService;
    private readonly IHubContext<ComentarioHub> _hubContext;

    public NoticiasController(ApplicationDbContext context, 
                              UserManager<IdentityUser> userManager, 
                              SignInManager<IdentityUser> signInManager,PerspectiveService perspectiveService,
                              IHubContext<ComentarioHub> hubContext) 
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _perspectiveService = perspectiveService;
        _hubContext = hubContext;
    }

 
    public async Task<IActionResult> Index()
    {
        var noticias = await _context.Noticias
                                      .OrderByDescending(n => n.EsFijada) 
                                    .ThenByDescending(n => n.FechaPublicacion) 
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
        noticia.Vistas++;
        _context.Update(noticia);
        await _context.SaveChangesAsync();

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
    // Cambia el parámetro para que acepte un objeto
    public async Task<IActionResult> ToggleFavorito([FromBody] ToggleFavoritoRequest request) 
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        // Usa request.NoticiaId en lugar de noticiaId
        var favoritoExistente = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.NoticiaId == request.NoticiaId && f.UsuarioId == userId);

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
                NoticiaId = request.NoticiaId, // <-- Usa request.NoticiaId
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
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Usuario no encontrado." });
        }
        bool esToxico = await _perspectiveService.EsComentarioToxico(textoComentario);
        if (esToxico)
        {
            // Si la IA lo detecta, lo rechaza
            return Json(new { success = false, message = "Tu comentario infringe las normas de la comunidad y no ha sido publicado." });
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
            AutorId = user.Id,
            AutorFotoUrl = fotoUrl
        };

        _context.Comentarios.Add(nuevoComentario);
        await _context.SaveChangesAsync();

        // --- INICIO DE LA MODIFICACIÓN DE SIGNALR ---

        // 1. Prepara los datos que se enviarán (deben coincidir con el JSON de retorno)
        var dataParaCliente = new
        {
            success = true,
            id = nuevoComentario.Id,
            autor = nuevoComentario.Autor,
            texto = nuevoComentario.Texto,
            fechaISO = nuevoComentario.FechaComentario.ToString("o"),
            fotoUrl = nuevoComentario.AutorFotoUrl 
        };

        // 2. Define el nombre del grupo para esta noticia
        string grupoNoticia = $"noticia-{noticiaId}";

        // 3. Envía el mensaje "RecibirComentario" a todos en ese grupo
        await _hubContext.Clients.Group(grupoNoticia).SendAsync("RecibirComentario", dataParaCliente);
        
        // --- FIN DE LA MODIFICACIÓN DE SIGNALR ---

        // 4. Devuelve la respuesta solo al cliente que envió el comentario
        return Json(dataParaCliente);
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
        return Json(new
        {
            success = true,
            texto = nuevoTexto,
            fechaISO = comentario.FechaComentario.ToString("o")
        });
    }
    // Añade estos métodos DENTRO de tu clase NoticiasController

// 1. GET: /Noticias/Administrador (Muestra la lista de noticias como en image_3cc995.png)
    [Authorize(Roles = "Admin")] // ¡Importante! Solo los admins pueden ver esto
    public async Task<IActionResult> Administrador()
    {
        var noticias = await _context.Noticias
                                    .OrderByDescending(n => n.EsFijada) 
                                    .ThenByDescending(n => n.FechaPublicacion) 
                                    .ToListAsync();
        // Esta acción usará una nueva vista: Views/Noticias/Administrador.cshtml
        return View(noticias);
    }

    // 2. GET: /Noticias/Crear (Muestra el formulario para crear una noticia nueva)
    [Authorize(Roles = "Admin")]
    public IActionResult Crear()
    {
        // Usará la vista: Views/Noticias/Crear.cshtml
        return View();
    }

    // 3. POST: /Noticias/Crear (Recibe los datos del formulario y guarda la noticia)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([Bind("Titulo,Contenido,UrlImagen")] Noticia noticia)
    {
        if (ModelState.IsValid)
        {
            noticia.FechaPublicacion = DateTime.UtcNow; // Pone la fecha actual
            _context.Add(noticia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Administrador)); // Vuelve al panel de admin
        }
        // Si hay un error, vuelve a mostrar el formulario con los datos
        return View(noticia);
    }

    // 4. GET: /Noticias/Editar/5 (Muestra el formulario de edición como en image_3cc977.png)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia == null)
        {
            return NotFound();
        }
        // Usará la vista: Views/Noticias/Editar.cshtml
        return View(noticia);
    }

// 5. POST: /Noticias/Editar/5 (Recibe los datos del formulario de edición y guarda los cambios)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Titulo,Contenido,UrlImagen,FechaPublicacion")] Noticia noticia)
    {
        if (id != noticia.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                noticia.FechaPublicacion = noticia.FechaPublicacion.ToUniversalTime();
                _context.Update(noticia);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Noticias.Any(e => e.Id == noticia.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Administrador));
        }
        return View(noticia);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var noticia = await _context.Noticias.FirstOrDefaultAsync(m => m.Id == id);
        if (noticia == null)
        {
            return NotFound();
        }
        // Usará la vista: Views/Noticias/Eliminar.cshtml
        return View(noticia);
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia != null)
        {
            _context.Noticias.Remove(noticia);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Administrador));
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Cambiamos la ruta para que acepte el ID (ej. /Noticias/AlternarFijado/5)
    [Route("Noticias/AlternarFijado/{id:int}")]
    public async Task<IActionResult> AlternarFijado(int id)
    {
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia == null)
        {
            return Json(new { success = false, message = "Noticia no encontrada." });
        }

        // Invierte el estado actual (si era true, lo vuelve false)
        noticia.EsFijada = !noticia.EsFijada;
        _context.Update(noticia);
        await _context.SaveChangesAsync();

        // Devuelve el nuevo estado para que el JavaScript actualice la vista
        return Json(new { success = true, esFijada = noticia.EsFijada });
    }
    // Pega esto dentro de tu clase NoticiasController

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DetalleAdmin(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var noticia = await _context.Noticias
                                    .Include(n => n.Comentarios) // Incluye los comentarios para contarlos
                                    .Include(n => n.Favoritos)   // Incluye los favoritos para contarlos
                                    .FirstOrDefaultAsync(m => m.Id == id);

        if (noticia == null)
        {
            return NotFound();
        }

        // (La vista 'DetalleAdmin.cshtml' se encargará de
        // inyectar el 'ConfiguracionSitioService' para obtener los colores)

        return View(noticia); // Devuelve la nueva vista 'DetalleAdmin.cshtml'
    }
        public class ToggleFavoritoRequest
    {
        public int NoticiaId { get; set; }
    }
}
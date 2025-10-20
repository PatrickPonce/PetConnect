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

public class NoticiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public NoticiasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var noticias = await _context.Noticias
                                      .OrderByDescending(n => n.FechaPublicacion)
                                      .ToListAsync();

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
        
        ViewBag.EsFavorito = false;

        if (User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User);
            bool esFavorito = await _context.Favoritos
                .AnyAsync(f => f.NoticiaId == noticia.Id && f.UsuarioId == userId);
            ViewBag.EsFavorito = esFavorito; 
        }

        return View(noticia);
    }

    // --- MÉTODO CORREGIDO ---
    // Este método [Authorize] (sin [HttpPost]) CARGA LA PÁGINA de favoritos.
    [Authorize] 
    public async Task<IActionResult> Favoritos()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "Home"); 
        }
        
        var noticiasFavoritas = await _context.Favoritos
            .Where(f => f.UsuarioId == userId)  
            .OrderByDescending(f => f.FechaAgregado) 
            .Select(f => f.Noticia) 
            .ToListAsync();
        
        return View(noticiasFavoritas);
    }

    // --- MÉTODO QUE FALTABA (RE-AGREGADO) ---
    // Este método [HttpPost] y [Authorize] GUARDA el favorito (el que llama el JavaScript).
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorito([FromBody] int noticiaId) // <-- Se usa [FromBody]
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
}
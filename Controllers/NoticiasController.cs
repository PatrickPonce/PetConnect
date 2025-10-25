using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.ViewModels; // Necesitaremos un ViewModel
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// Definimos un ViewModel para la Noticia
namespace PetConnect.ViewModels
{
    public class NoticiaViewModel
    {
        public Noticia Noticia { get; set; }
        public bool EsFavorito { get; set; }
    }
}

public class NoticiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public NoticiasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // --- ACCIÓN INDEX (LISTA) ACTUALIZADA ---
    public async Task<IActionResult> Index()
    {
        var noticiasQuery = _context.Noticias.AsQueryable();
        var noticiaViewModels = new List<NoticiaViewModel>();
        
        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoritosUsuarioIds = await _context.FavoritosNoticia
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.NoticiaId)
                .ToHashSetAsync();

            foreach (var noticia in await noticiasQuery.AsNoTracking().ToListAsync())
            {
                noticiaViewModels.Add(new NoticiaViewModel
                {
                    Noticia = noticia,
                    EsFavorito = favoritosUsuarioIds.Contains(noticia.Id)
                });
            }
        }
        else
        {
            foreach (var noticia in await noticiasQuery.AsNoTracking().ToListAsync())
            {
                noticiaViewModels.Add(new NoticiaViewModel
                {
                    Noticia = noticia,
                    EsFavorito = false
                });
            }
        }

        return View(noticiaViewModels);
    }

    // --- ACCIÓN DETALLE ACTUALIZADA ---
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var noticia = await _context.Noticias
            .Include(n => n.Comentarios) // Carga los comentarios
            .FirstOrDefaultAsync(n => n.Id == id);

        if (noticia == null)
        {
            return NotFound();
        }
        
        // Pasamos el modelo Noticia simple al detalle
        return View(noticia); 
    }

    // --- NUEVO ENDPOINT PARA FAVORITOS ---
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorito(int noticiaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var favoritoExistente = await _context.FavoritosNoticia
            .FirstOrDefaultAsync(f => f.NoticiaId == noticiaId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosNoticia.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            var nuevoFavorito = new FavoritoNoticia
            {
                NoticiaId = noticiaId,
                UsuarioId = userId
            };
            _context.FavoritosNoticia.Add(nuevoFavorito);
            agregado = true;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, agregado = agregado });
    }
}
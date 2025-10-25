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

        return View(noticia);
    }

   
    [HttpPost]
    [Authorize] 
    public async Task<IActionResult> ToggleFavorito(int noticiaId)
    {
       
        var userId = _userManager.GetUserId(User); 

        var favoritoExistente = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.NoticiaId == noticiaId && f.UsuarioId == userId);

        if (favoritoExistente != null)
        {

            _context.Favoritos.Remove(favoritoExistente);
        }
        else
        {
            var nuevoFavorito = new Favorito
            {
                NoticiaId = noticiaId,
                UsuarioId = userId!,
                FechaAgregado = DateTime.UtcNow
            };
            _context.Favoritos.Add(nuevoFavorito);
        }
        
        await _context.SaveChangesAsync();

        return RedirectToAction("Detalle", new { id = noticiaId });
    }
}
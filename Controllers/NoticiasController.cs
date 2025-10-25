using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using PetConnect.Data;
using PetConnect.Models; 
using System; 
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 

public class NoticiasController : Controller
{
    private readonly ApplicationDbContext _context;

    public NoticiasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var noticias = await _context.Noticias
                                     .OrderByDescending(n => n.FechaPublicacion)
                                     .ToListAsync();

        foreach (var noticia in noticias)
        {
            
            string contenidoLimpio = Regex.Replace(noticia.Contenido ?? string.Empty, "<.*?>", String.Empty);
            
            // 2. Lógica segura para crear el resumen
            if (contenidoLimpio.Length > 150)
            {
                
                noticia.Contenido = contenidoLimpio.Substring(0, 150) + "...";
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
}
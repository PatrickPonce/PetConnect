using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PetConnect.Data;
using PetConnect.Models;
using System.Threading.Tasks;

[Authorize]
public class ComentariosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ComentariosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> AgregarComentario(int NoticiaId, string Texto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
           
            return Unauthorized(); 
        }

        var comentario = new Comentario
        {
            NoticiaId = NoticiaId,
            Texto = Texto,
            Autor = user.UserName,
            FechaComentario = DateTime.Now
        };

        _context.Comentarios.Add(comentario);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detalle", "Noticias", new { id = NoticiaId });
    }
}
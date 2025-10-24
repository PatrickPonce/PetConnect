using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;

    public FavoritesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var viewModel = new FavoritosViewModel();

        var lugaresQuery = _context.FavoritosLugar
            .Where(f => f.UsuarioId == userId)
            .Select(f => f.LugarPetFriendly);

        if (!string.IsNullOrEmpty(searchString))
        {
            lugaresQuery = lugaresQuery.Where(l => l.Nombre.Contains(searchString) || l.Ubicacion.Contains(searchString));
        }

        viewModel.LugaresFavoritos = await lugaresQuery.ToListAsync();

        return View(viewModel);
    }
}
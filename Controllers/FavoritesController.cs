using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
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

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var favoritedLugares = await _context.FavoritosLugar
            .Where(f => f.UsuarioId == userId)
            .Include(f => f.LugarPetFriendly)
            .Select(f => f.LugarPetFriendly)
            .ToListAsync();

        return View(favoritedLugares);
    }
}
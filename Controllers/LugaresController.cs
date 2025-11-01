using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class LugaresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LugaresController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string searchString, int? pageNumber)
    {
        var lugaresQuery = _context.LugaresPetFriendly.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            lugaresQuery = lugaresQuery.Where(l => l.Nombre.Contains(searchString) || l.Ubicacion.Contains(searchString));
        }

        int pageSize = 9;
        int currentPage = pageNumber ?? 1;

        var paginatedLugares = await PaginatedList<LugarPetFriendly>.CreateAsync(lugaresQuery.AsNoTracking(), currentPage, pageSize);

        var lugarViewModels = new List<LugarViewModel>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoritosUsuarioIds = await _context.FavoritosLugar
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.LugarPetFriendlyId)
                .ToHashSetAsync();

            foreach (var lugar in paginatedLugares)
            {
                lugarViewModels.Add(new LugarViewModel
                {
                    Lugar = lugar,
                    EsFavorito = favoritosUsuarioIds.Contains(lugar.Id)
                });
            }
        }
        else
        {
            foreach (var lugar in paginatedLugares)
            {
                lugarViewModels.Add(new LugarViewModel
                {
                    Lugar = lugar,
                    EsFavorito = false
                });
            }
        }

        var paginatedViewModel = new PaginatedList<LugarViewModel>(
            lugarViewModels,
            await lugaresQuery.CountAsync(),
            currentPage,
            pageSize
        );

        ViewData["CurrentFilter"] = searchString;
        return View(paginatedViewModel);
    }

    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var lugar = await _context.LugaresPetFriendly
            .Include(l => l.Comentarios)
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (lugar == null)
        {
            return NotFound();
        }

        return View(lugar);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int lugarId, string textoComentario)
    {
        if (string.IsNullOrWhiteSpace(textoComentario))
        {
            return Json(new { success = false, message = "El comentario no puede estar vacío." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuario = await _userManager.FindByIdAsync(userId);

        var comentario = new ComentarioLugar
        {
            Texto = textoComentario,
            FechaComentario = DateTime.UtcNow,
            LugarPetFriendlyId = lugarId,
            UsuarioId = userId
        };

        _context.ComentariosLugar.Add(comentario);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = "Comentario añadido.",
            autor = usuario.UserName,
            texto = comentario.Texto,
            fecha = comentario.FechaComentario.ToString("g")
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarComentario(int comentarioId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comentario = await _context.ComentariosLugar.FindAsync(comentarioId);

        if (comentario == null)
        {
            return NotFound();
        }

        if (comentario.UsuarioId != userId)
        {
            return Forbid();
        }

        _context.ComentariosLugar.Remove(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    // 1. CAMBIA LA FIRMA para que acepte el objeto [FromBody]
    public async Task<IActionResult> ToggleFavorito([FromBody] ToggleLugarRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // 2. USA request.LugarId en lugar de solo lugarId
        var favoritoExistente = await _context.FavoritosLugar
            .FirstOrDefaultAsync(f => f.LugarPetFriendlyId == request.LugarId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosLugar.Remove(favoritoExistente);
            agregado = false; // Se quitó
        }
        else
        {
            var nuevoFavorito = new FavoritoLugar
            {
                // 3. USA request.LugarId aquí también
                LugarPetFriendlyId = request.LugarId,
                UsuarioId = userId
            };
            _context.FavoritosLugar.Add(nuevoFavorito);
            agregado = true; // Se añadió
        }

        await _context.SaveChangesAsync();
        
        // El JS espera 'agregado = false' para saber que se quitó
        return Json(new { success = true, agregado = agregado });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken] // ¡Importante para seguridad!
    public async Task<IActionResult> EliminarFavoritos([FromBody] List<int> lugarIds)
    {
        if (lugarIds == null || !lugarIds.Any())
        {
            return Json(new { success = false, message = "No se seleccionaron lugares." });
        }

        // Usar FindFirstValue es más directo que UserManager si solo necesitas el ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        // Busca en la tabla 'FavoritosLugar' (o como se llame tu tabla join)
        var favoritosAEliminar = await _context.FavoritosLugar
            .Where(f => f.UsuarioId == userId && lugarIds.Contains(f.LugarPetFriendlyId)) // <-- Ajusta 'LugarPetFriendlyId' al nombre real de tu FK
            .ToListAsync();

        if (favoritosAEliminar.Any())
        {
            _context.FavoritosLugar.RemoveRange(favoritosAEliminar);
            await _context.SaveChangesAsync();
        }

        // Devuelve siempre success = true, incluso si no se encontró nada que borrar
        return Json(new { success = true });
    }
}
// Pega esto FUERA de la clase LugaresController, al final del archivo
public class ToggleLugarRequest
{
    public int LugarId { get; set; }
}
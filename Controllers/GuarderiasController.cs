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

public class GuarderiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public GuarderiasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string searchString, int? pageNumber)
    {
        var guarderiasQuery = _context.Guarderias.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            guarderiasQuery = guarderiasQuery.Where(g => g.Nombre.Contains(searchString) || g.Ubicacion.Contains(searchString));
        }

        int pageSize = 9;
        int currentPage = pageNumber ?? 1;

        var paginatedGuarderias = await PaginatedList<Guarderia>.CreateAsync(guarderiasQuery.AsNoTracking(), currentPage, pageSize);

        var guarderiaViewModels = new List<GuarderiaViewModel>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoritosUsuarioIds = await _context.FavoritosGuarderia
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.GuarderiaId)
                .ToHashSetAsync();

            foreach (var guarderia in paginatedGuarderias)
            {
                guarderiaViewModels.Add(new GuarderiaViewModel
                {
                    Guarderia = guarderia,
                    EsFavorito = favoritosUsuarioIds.Contains(guarderia.Id)
                });
            }
        }
        else
        {
            foreach (var guarderia in paginatedGuarderias)
            {
                guarderiaViewModels.Add(new GuarderiaViewModel
                {
                    Guarderia = guarderia,
                    EsFavorito = false
                });
            }
        }
        
        var paginatedViewModel = new PaginatedList<GuarderiaViewModel>(
            guarderiaViewModels, 
            await guarderiasQuery.CountAsync(), 
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

        var guarderia = await _context.Guarderias
            .Include(g => g.Comentarios)
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (guarderia == null)
        {
            return NotFound();
        }

        return View(guarderia);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int guarderiaId, string textoComentario)
    {
        if (string.IsNullOrWhiteSpace(textoComentario))
        {
            return Json(new { success = false, message = "El comentario no puede estar vacío." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuario = await _userManager.FindByIdAsync(userId);

        var comentario = new ComentarioGuarderia
        {
            Texto = textoComentario,
            FechaComentario = DateTime.UtcNow,
            GuarderiaId = guarderiaId,
            UsuarioId = userId
        };

        _context.ComentariosGuarderia.Add(comentario);
        await _context.SaveChangesAsync();

        return Json(new {
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
        var comentario = await _context.ComentariosGuarderia.FindAsync(comentarioId);

        if (comentario == null)
        {
            return NotFound();
        }

        if (comentario.UsuarioId != userId)
        {
            return Forbid();
        }

        _context.ComentariosGuarderia.Remove(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorito(int guarderiaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var favoritoExistente = await _context.FavoritosGuarderia
            .FirstOrDefaultAsync(f => f.GuarderiaId == guarderiaId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosGuarderia.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            var nuevoFavorito = new FavoritoGuarderia
            {
                GuarderiaId = guarderiaId,
                UsuarioId = userId
            };
            _context.FavoritosGuarderia.Add(nuevoFavorito);
            agregado = true;
        }

        await _context.SaveChangesAsync();
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
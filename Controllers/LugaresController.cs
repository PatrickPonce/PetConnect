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

    public async Task<IActionResult> Index(string searchString)
    {
        var lugaresQuery = _context.LugaresPetFriendly.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            lugaresQuery = lugaresQuery.Where(l => l.Nombre.Contains(searchString) || l.Ubicacion.Contains(searchString));
        }

        var lugares = await lugaresQuery.ToListAsync();
        var lugarViewModels = new List<LugarViewModel>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoritosUsuarioIds = await _context.FavoritosLugar
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.LugarPetFriendlyId)
                .ToHashSetAsync();

            lugarViewModels = lugares.Select(lugar => new LugarViewModel
            {
                Lugar = lugar,
                EsFavorito = favoritosUsuarioIds.Contains(lugar.Id)
            }).ToList();
        }
        else
        {
            lugarViewModels = lugares.Select(lugar => new LugarViewModel
            {
                Lugar = lugar,
                EsFavorito = false
            }).ToList();
        }

        ViewData["CurrentFilter"] = searchString;
        return View(lugarViewModels);
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
    public async Task<IActionResult> ToggleFavorito(int lugarId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var favoritoExistente = await _context.FavoritosLugar
            .FirstOrDefaultAsync(f => f.LugarPetFriendlyId == lugarId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosLugar.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            var nuevoFavorito = new FavoritoLugar
            {
                LugarPetFriendlyId = lugarId,
                UsuarioId = userId
            };
            _context.FavoritosLugar.Add(nuevoFavorito);
            agregado = true;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, agregado = agregado });
    }
}
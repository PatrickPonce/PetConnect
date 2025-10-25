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

public class PetShopsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PetShopsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string searchString, int? pageNumber)
    {
        var petShopsQuery = _context.PetShops.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            petShopsQuery = petShopsQuery.Where(p => p.Nombre.Contains(searchString) || p.Ubicacion.Contains(searchString));
        }

        int pageSize = 9;
        var paginatedPetShops = await PaginatedList<PetShop>.CreateAsync(petShopsQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

        var petShopViewModels = new List<PetShopViewModel>();
        var userId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

        if (userId != null)
        {
            var favoritosUsuarioIds = await _context.FavoritosPetShop
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.PetShopId)
                .ToHashSetAsync();

            foreach (var petShop in paginatedPetShops)
            {
                petShopViewModels.Add(new PetShopViewModel
                {
                    PetShop = petShop,
                    EsFavorito = favoritosUsuarioIds.Contains(petShop.Id)
                });
            }
        }
        else
        {
            foreach (var petShop in paginatedPetShops)
            {
                petShopViewModels.Add(new PetShopViewModel
                {
                    PetShop = petShop,
                    EsFavorito = false
                });
            }
        }
        
        // ESTA LÍNEA AHORA FUNCIONARÁ CORRECTAMENTE
        var paginatedViewModel = new PaginatedList<PetShopViewModel>(
            petShopViewModels, 
            paginatedPetShops.TotalCount, 
            paginatedPetShops.PageIndex, 
            pageSize
        );

        ViewData["CurrentFilter"] = searchString;
        return View(paginatedViewModel);
    }
    
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null) return NotFound();

        var petShop = await _context.PetShops
            .Include(p => p.Comentarios)
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (petShop == null) return NotFound();

        return View(petShop);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int petShopId, string textoComentario)
    {
        if (string.IsNullOrWhiteSpace(textoComentario))
        {
            return Json(new { success = false, message = "El comentario no puede estar vacío." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuario = await _userManager.FindByIdAsync(userId);

        var comentario = new ComentarioPetShop
        {
            Texto = textoComentario,
            FechaComentario = DateTime.UtcNow,
            PetShopId = petShopId,
            UsuarioId = userId
        };

        _context.ComentariosPetShop.Add(comentario);
        await _context.SaveChangesAsync();

        return Json(new {
            success = true,
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
        var comentario = await _context.ComentariosPetShop.FindAsync(comentarioId);

        if (comentario == null) return NotFound();
        if (comentario.UsuarioId != userId) return Forbid();

        _context.ComentariosPetShop.Remove(comentario);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorito(int petShopId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var favoritoExistente = await _context.FavoritosPetShop
            .FirstOrDefaultAsync(f => f.PetShopId == petShopId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosPetShop.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            _context.FavoritosPetShop.Add(new FavoritoPetShop { PetShopId = petShopId, UsuarioId = userId });
            agregado = true;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, agregado = agregado });
    }
}
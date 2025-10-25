using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Claims;
using PetConnect.ViewModels;
using System; // Necesario para DateTimeOffset
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class UsuariosAdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsuariosAdminController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // --- ACCIÓN INDEX ACTUALIZADA CON FILTROS ---
    public async Task<IActionResult> Index(string searchString, string statusFilter)
    {
        ViewData["CurrentNameFilter"] = searchString; // Para mantener el valor en la vista
        ViewData["CurrentStatusFilter"] = statusFilter; // Para mantener el valor en la vista

        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Empezamos con la consulta base (excluyendo al admin)
        var usersQuery = _userManager.Users.Where(u => u.Id != currentAdminId);

        // 1. Aplicar filtro por Nombre (si existe)
        if (!string.IsNullOrEmpty(searchString))
        {
            usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString) || (u.Email != null && u.Email.Contains(searchString)));
        }

        // 2. Aplicar filtro por Estado (si existe y es válido)
        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "bloqueado")
            {
                // Bloqueado = LockoutEnd está en el futuro
                usersQuery = usersQuery.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
            }
            else if (statusFilter == "activo")
            {
                // Activo = LockoutEnd es nulo o está en el pasado
                usersQuery = usersQuery.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
            }
            // Si es "todos", no se aplica filtro de estado
        }
        
        // Ejecutamos la consulta filtrada
        var users = await usersQuery.ToListAsync();

        var userViewModels = new List<UsuarioViewModel>();
        foreach (var user in users)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var profilePicUrl = claims.FirstOrDefault(c => c.Type == PetConnectClaimTypes.ProfilePictureUrl)?.Value;
            
            // Calculamos si está bloqueado
            bool isLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

            userViewModels.Add(new UsuarioViewModel
            {
                Id = user.Id,
                NombreUsuario = user.UserName,
                Email = user.Email,
                NumeroTelefono = user.PhoneNumber,
                ProfilePictureUrl = profilePicUrl,
                IsLockedOut = isLockedOut // <-- Pasamos el estado
            });
        }

        return View(userViewModels);
    }

    // --- ACCIÓN BLOQUEAR/DESBLOQUEAR ACTUALIZADA ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBloqueoUsuario(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        // Verificamos si ya está bloqueado (LockoutEnd en el futuro)
        bool isCurrentlyLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

        IdentityResult result;
        if (isCurrentlyLockedOut)
        {
            // Si está bloqueado, lo desbloqueamos (poniendo la fecha en el pasado o null)
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1)); // O null
            TempData["SuccessMessage"] = $"Usuario '{user.UserName}' desbloqueado.";
        }
        else
        {
            // Si no está bloqueado, lo bloqueamos indefinidamente (fecha muy lejana)
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["SuccessMessage"] = $"Usuario '{user.UserName}' bloqueado.";
        }
        
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Ocurrió un error al cambiar el estado de bloqueo.";
            // Podrías loggear los errores: foreach (var error in result.Errors) { ... }
        }

        return RedirectToAction(nameof(Index));
    }


    // --- ACCIÓN ELIMINAR (SIN CAMBIOS FUNCIONALES POR AHORA) ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
             TempData["ErrorMessage"] = "Usuario no encontrado.";
             return RedirectToAction(nameof(Index));
        }
        
        // --- IMPLEMENTACIÓN REAL (DESCOMENTAR CON CUIDADO) ---
        // var result = await _userManager.DeleteAsync(user);
        // if (result.Succeeded)
        // {
        //     TempData["SuccessMessage"] = $"Usuario '{user.UserName}' eliminado permanentemente.";
        // }
        // else
        // {
        //     TempData["ErrorMessage"] = "Error al eliminar el usuario.";
        //     // Log errors: foreach (var error in result.Errors) { ... }
        // }
        // --- FIN IMPLEMENTACIÓN REAL ---

        TempData["SuccessMessage"] = $"Usuario '{user.UserName}' eliminado (funcionalidad pendiente)."; // Mensaje temporal
        return RedirectToAction(nameof(Index));
    }
}
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
using System.Security.Cryptography; // <-- AÑADIR ESTA LÍNEA
using System.Text; // <-- AÑADIR ESTA LÍNEA
using Microsoft.Extensions.Configuration; // Para leer la API Key
using System.Net.Http; // Para HttpClient
using System.Text.Json; // Para deserializar
using PetConnect.Models.Api; // Para el modelo de respuesta que creamos

[Authorize(Roles = "Admin")]
public class UsuariosAdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration; // 2. Inyecta IConfiguration
    private readonly IHttpClientFactory _httpClientFactory;

    public UsuariosAdminController(UserManager<IdentityUser> userManager, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    // --- ACCIÓN INDEX ACTUALIZADA CON FILTROS ---
    public async Task<IActionResult> Index(string searchString, string statusFilter)
    {
        ViewData["CurrentNameFilter"] = searchString;
        ViewData["CurrentStatusFilter"] = statusFilter;

        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var usersQuery = _userManager.Users.Where(u => u.Id != currentAdminId);

        if (!string.IsNullOrEmpty(searchString))
        {
            usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString) || (u.Email != null && u.Email.Contains(searchString)));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "bloqueado")
            {
                usersQuery = usersQuery.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
            }
            else if (statusFilter == "activo")
            {
                usersQuery = usersQuery.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
            }
        }
        
        var users = await usersQuery.ToListAsync();

        var userViewModels = new List<UsuarioViewModel>();
        foreach (var user in users)
        {
            // 1. Obtiene el claim de la foto de perfil (sin cambios)
            var claims = await _userManager.GetClaimsAsync(user);
            var profilePicUrl = claims.FirstOrDefault(c => c.Type == PetConnectClaimTypes.ProfilePictureUrl)?.Value;

            // --- INICIO DE LA LÓGICA GRAVATAR ---
            // 2. Si NO hay foto de perfil (profilePicUrl está vacío) Y SÍ hay un email
            if (string.IsNullOrEmpty(profilePicUrl) && !string.IsNullOrEmpty(user.Email))
            {
                // 3. Genera la URL de Gravatar
                var emailHash = CreateGravatarHash(user.Email);
                // "d=mp" es el parámetro que le dice a Gravatar que muestre un ícono genérico si no encuentra un avatar
                profilePicUrl = $"https://www.gravatar.com/avatar/{emailHash}?d=mp";
            }
            // --- FIN DE LA LÓGICA GRAVATAR ---

            // --- INICIO: AÑADIR LECTURA DE CLAIM IP ---
            var regIpClaim = claims.FirstOrDefault(c => c.Type == PetConnectClaimTypes.RegistrationIpAddress);
            // --- FIN: LECTURA DE CLAIM IP ---

            bool isLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

            userViewModels.Add(new UsuarioViewModel
            {
                Id = user.Id,
                NombreUsuario = user.UserName,
                Email = user.Email,
                NumeroTelefono = user.PhoneNumber,
                ProfilePictureUrl = profilePicUrl, // <-- Aquí se asigna la foto personalizada O el Gravatar
                IsLockedOut = isLockedOut,
                RegistrationIp = regIpClaim?.Value
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

        // --- INICIO DE LA LÓGICA DE ELIMINACIÓN REAL ---
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Usuario '{user.UserName}' eliminado permanentemente.";
        }
        else
        {
            TempData["ErrorMessage"] = "Error al eliminar el usuario.";
            // Opcional: puedes registrar los errores si quieres ver por qué falló
            // foreach (var error in result.Errors) { _logger.LogError(error.Description); }
        }
        // --- FIN DE LA LÓGICA DE ELIMINACIÓN REAL ---

        // Eliminamos el mensaje temporal
        // TempData["SuccessMessage"] = $"Usuario '{user.UserName}' eliminado (funcionalidad pendiente).";

        return RedirectToAction(nameof(Index));
    }

    // --- MÉTODO DE AYUDA PARA GRAVATAR ---
    private string CreateGravatarHash(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return string.Empty;
        }

        // Usar MD5 para hashear el email en minúsculas y sin espacios
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(email.Trim().ToLowerInvariant());
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convertir el byte array a un string hexadecimal
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
    
    // 5. --- AÑADE ESTA NUEVA ACCIÓN ---
    [HttpGet]
    public async Task<IActionResult> ObtenerInfoCompletaIP(string ip)
    {
        if (string.IsNullOrEmpty(ip) || ip == "::1" || ip == "127.0.0.1")
        {
            return BadRequest(new { message = "IP no válida o es local." });
        }

        // Lee la API Key de forma segura (funciona local y en Render)
        var apiKey = _configuration["ProxyCheck:ApiKey"]; 
        if (string.IsNullOrEmpty(apiKey))
        {
            return StatusCode(500, new { message = "API Key de ProxyCheck no está configurada en el servidor." });
        }

        var url = $"http://proxycheck.io/v2/{ip}?key={apiKey}&vpn=1&asn=1";
        
        try
        {
            var client = _httpClientFactory.CreateClient(); // Usa HttpClientFactory
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "Error al contactar la API de ProxyCheck." });
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            
            // Deserializa la respuesta de proxycheck (ej. {"status":"ok", "proxy":"no", ...})
            var proxyCheckData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

            // También deserializa nuestra clase tipada para obtener los datos principales
            var typedData = JsonSerializer.Deserialize<ProxyCheckResponse>(jsonString);
            
            // Devuelve los datos que la vista necesita
            return Ok(typedData);
        }
        catch (Exception ex)
        {
            // Loggear el error real (ex)
            return StatusCode(500, new { message = "Error interno del servidor al procesar la IP." });
        }
    }
}
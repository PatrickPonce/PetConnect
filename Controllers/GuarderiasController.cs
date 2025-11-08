using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.Services;
using PetConnect.ViewModels;
using System.Security.Claims;

public class GuarderiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;

    public GuarderiasController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index(string searchString, int? pageNumber)
    {
        var guarderiasQuery = _context.Guarderias.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            guarderiasQuery = guarderiasQuery.Where(g => g.Nombre.Contains(searchString) || g.Ubicacion.Contains(searchString));
        }

        int pageSize = 9;
        var paginatedGuarderias = await PaginatedList<Guarderia>.CreateAsync(guarderiasQuery.AsNoTracking(), pageNumber ?? 1, pageSize);
        var guarderiaViewModels = new List<GuarderiaViewModel>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoritosUsuarioIds = await _context.FavoritosGuarderia.Where(f => f.UsuarioId == userId).Select(f => f.GuarderiaId).ToHashSetAsync();
            foreach (var guarderia in paginatedGuarderias)
            {
                guarderiaViewModels.Add(new GuarderiaViewModel { Guarderia = guarderia, EsFavorito = favoritosUsuarioIds.Contains(guarderia.Id) });
            }
        }
        else
        {
            foreach (var guarderia in paginatedGuarderias)
            {
                guarderiaViewModels.Add(new GuarderiaViewModel { Guarderia = guarderia, EsFavorito = false });
            }
        }
        
        var paginatedViewModel = new PaginatedList<GuarderiaViewModel>(guarderiaViewModels, await guarderiasQuery.CountAsync(), pageNumber ?? 1, pageSize);

        ViewData["CurrentFilter"] = searchString;
        return View(paginatedViewModel);
    }
    
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null) return NotFound();
        var guarderia = await _context.Guarderias.Include(g => g.Comentarios).ThenInclude(c => c.Usuario).FirstOrDefaultAsync(m => m.Id == id);
        if (guarderia == null) return NotFound();
        return View(guarderia);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int guarderiaId, string textoComentario)
    {
        if (string.IsNullOrWhiteSpace(textoComentario)) return Json(new { success = false, message = "El comentario no puede estar vacío." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comentario = new ComentarioGuarderia { Texto = textoComentario, FechaComentario = DateTime.UtcNow, GuarderiaId = guarderiaId, UsuarioId = userId };
        _context.ComentariosGuarderia.Add(comentario);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Comentario añadido." });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarComentario(int comentarioId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comentario = await _context.ComentariosGuarderia.FindAsync(comentarioId);
        if (comentario == null) return NotFound();
        if (comentario.UsuarioId != userId) return Forbid();
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
        var favoritoExistente = await _context.FavoritosGuarderia.FirstOrDefaultAsync(f => f.GuarderiaId == guarderiaId && f.UsuarioId == userId);
        bool agregado;
        if (favoritoExistente != null)
        {
            _context.FavoritosGuarderia.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            _context.FavoritosGuarderia.Add(new FavoritoGuarderia { GuarderiaId = guarderiaId, UsuarioId = userId });
            agregado = true;
        }
        await _context.SaveChangesAsync();
        return Json(new { success = true, agregado = agregado });
    }
    
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarFavoritos([FromBody] List<int> lugarIds) 
    {
        if (lugarIds == null || !lugarIds.Any()) return Json(new { success = false, message = "No se seleccionaron lugares." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        var favoritosAEliminar = await _context.FavoritosLugar.Where(f => f.UsuarioId == userId && lugarIds.Contains(f.LugarPetFriendlyId)).ToListAsync();
        if (favoritosAEliminar.Any())
        {
            _context.FavoritosLugar.RemoveRange(favoritosAEliminar);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true }); 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarSolicitudCita([FromForm] CitaViewModel model)
    {
        if (ModelState.IsValid)
        {
            var guarderia = await _context.Guarderias.FindAsync(model.GuarderiaId);
            if (guarderia == null) return NotFound(new { success = false, message = "Guardería no encontrada." });

            try
            {
                var subjectCliente = "Hemos recibido tu solicitud de cita en PetConnect";
                var contentCliente = $@"<h1>¡Hola, {model.NombreCliente}!</h1><p>Recibimos tu solicitud de cita para <strong>{guarderia.Nombre}</strong> para el día <strong>{model.Fecha:dd/MM/yyyy}</strong>.</p><p>El equipo de la guardería se pondrá en contacto contigo a la brevedad para confirmar la disponibilidad.</p><p>¡Gracias por usar PetConnect!</p>";
                await _emailService.SendEmailAsync(model.EmailCliente, subjectCliente, contentCliente);

                var emailDueño = "correo.dueño.guarderia@ejemplo.com"; 
                var subjectDueño = $"Nueva Solicitud de Cita de {model.NombreCliente}";
                var contentDueño = $@"<h1>¡Nueva Solicitud de Cita!</h1><p>Has recibido una nueva solicitud a través de PetConnect:</p><ul><li><strong>Cliente:</strong> {model.NombreCliente}</li><li><strong>Email:</strong> {model.EmailCliente}</li><li><strong>Fecha Solicitada:</strong> {model.Fecha:dd/MM/yyyy}</li><li><strong>Mensaje:</strong> {(string.IsNullOrEmpty(model.Mensaje) ? "No se incluyó mensaje." : model.Mensaje)}</li></ul><p>Por favor, ponte en contacto con el cliente para confirmar.</p>";
                await _emailService.SendEmailAsync(emailDueño, subjectDueño, contentDueño);

                return Json(new { success = true, message = "¡Solicitud enviada! Revisa tu correo para la confirmación." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Hubo un error al enviar el correo. Por favor, intenta de nuevo." });
            }
        }
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return BadRequest(new { success = false, message = "Los datos proporcionados no son válidos.", errors = errors });
    }
}
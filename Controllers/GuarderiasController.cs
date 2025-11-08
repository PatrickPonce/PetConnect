using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.Services;
using PetConnect.ViewModels;
using System.Globalization; // <-- Añadido para asegurar el formato de fecha correcto
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
            
            // Opcional: Si tienes un modelo 'Reserva', aquí es donde lo guardarías
            // var nuevaReserva = new Reserva { ... };
            // _context.Reservas.Add(nuevaReserva);
            // await _context.SaveChangesAsync();

            try
            {

                var urlImagenLogo = "https://www.google.com/url?sa=i&url=https%3A%2F%2Far.pinterest.com%2Fpin%2F530650768612557378%2F&psig=AOvVaw1rYHRPSG1gvycVc6JIyo36&ust=1762668932491000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCNi3qs_z4ZADFQAAAAAdAAAAABAo"; // <-- CAMBIA ESTA URL por la de tu logo

                // Para mostrar la fecha en español (ej. "lunes, 15 de enero de 2024")
                var culturaEspañol = new CultureInfo("es-ES");

                // --- Correo para el Cliente ---
                var subjectCliente = $"Confirmación de tu solicitud de cita en {guarderia.Nombre}";
                var contentCliente = $@"
                    <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; padding: 20px;'>
                        <img src='{urlImagenLogo}' alt='Logo de PetConnect' style='max-width: 150px; height: auto; display: block; margin-bottom: 20px;'/>
                        <h2 style='color: #4A4A4A;'>¡Hola, {model.NombreCliente}!</h2>
                        <p>Hemos recibido correctamente tu solicitud de cita para <strong style='color: #5a4fcf;'>{guarderia.Nombre}</strong>.</p>
                        <p>Estos son los detalles que registramos:</p>
                        <div style='background-color: #f7f7f7; padding: 15px; border-radius: 5px;'>
                            <ul style='list-style: none; padding: 0;'>
                                <li><strong>Fecha:</strong> <span style='color: #d63384; font-weight: bold;'>{model.Fecha.ToString("dddd, dd 'de' MMMM 'de' yyyy", culturaEspañol)}</span></li>
                                <li><strong>Hora:</strong> <span style='color: #d63384; font-weight: bold;'>{model.Hora:hh\\:mm}</span></li>
                            </ul>
                        </div>
                        <p>El personal de la guardería se pondrá en contacto contigo a la brevedad para confirmar la disponibilidad y los siguientes pasos.</p>
                        <p style='margin-top: 30px; font-size: 0.9em; color: #777;'>Atentamente,<br>El equipo de Purr & Paws</p>
                    </div>";
                
                await _emailService.SendEmailAsync(model.EmailCliente, subjectCliente, contentCliente);

                // --- Correo para el Dueño de la Guardería ---
                var emailDueño = "correo.dueño.guarderia@ejemplo.com"; // <-- CAMBIA ESTO por el email real del negocio
                var subjectDueño = $"Nueva Solicitud de Cita: {model.NombreCliente} para el {model.Fecha:dd/MM/yyyy}";
                var contentDueño = $@"<h1>¡Nueva Solicitud de Cita!</h1><p>Has recibido una nueva solicitud:</p><ul><li><strong>Cliente:</strong> {model.NombreCliente} ({model.EmailCliente})</li><li><strong>Fecha y Hora:</strong> {model.Fecha.ToString("dd/MM/yyyy")} a las {model.Hora:hh\\:mm}</li><li><strong>Mensaje:</strong> {(string.IsNullOrEmpty(model.Mensaje) ? "N/A" : model.Mensaje)}</li></ul><p>Por favor, ponte en contacto con el cliente para confirmar.</p>";
                
                await _emailService.SendEmailAsync(emailDueño, subjectDueño, contentDueño);

                return Json(new { success = true, message = "¡Solicitud enviada! Revisa tu correo para más detalles." });
            }
            catch (Exception ex)
            {
                // Para depuración, puedes registrar el error
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { success = false, message = "Hubo un error al enviar la notificación por correo." });
            }
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return BadRequest(new { success = false, message = "Por favor, corrige los errores.", errors = errors });
    }
}
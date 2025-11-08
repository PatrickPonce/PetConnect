// Archivo: Controllers/GuarderiasController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.Services;
using PetConnect.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList; // <-- ¡IMPORTANTE! Este 'using' es necesario para la nueva paginación.

namespace PetConnect.Controllers // <-- Añadido el namespace para buena práctica
{
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

        // --- ESTA ES LA ÚNICA ACCIÓN QUE HA SIDO MODIFICADA PARA SOLUCIONAR EL ERROR ---
        public async Task<IActionResult> Index(string busqueda, int? page)
        {
            ViewData["BusquedaActual"] = busqueda;
            
            var guarderiasQuery = _context.Guarderias.OrderBy(g => g.Nombre).AsNoTracking();

            if (!string.IsNullOrEmpty(busqueda))
            {
                guarderiasQuery = guarderiasQuery.Where(g => g.Nombre.ToLower().Contains(busqueda.ToLower()));
            }
            
            int pageSize = 6; // Puedes ajustar el número de items por página
            var paginatedGuarderias = await guarderiasQuery.ToPagedListAsync(page ?? 1, pageSize);

            var viewModels = new List<GuarderiaViewModel>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var favoritosIds = await _context.FavoritosGuarderia
                    .Where(f => f.UsuarioId == userId)
                    .Select(f => f.GuarderiaId)
                    .ToHashSetAsync();
                
                foreach (var guarderia in paginatedGuarderias)
                {
                    viewModels.Add(new GuarderiaViewModel { Guarderia = guarderia, EsFavorito = favoritosIds.Contains(guarderia.Id) });
                }
            }
            else
            {
                foreach (var guarderia in paginatedGuarderias)
                {
                    viewModels.Add(new GuarderiaViewModel { Guarderia = guarderia, EsFavorito = false });
                }
            }
            
            var paginatedViewModel = new StaticPagedList<GuarderiaViewModel>(viewModels, paginatedGuarderias.GetMetaData());

            return View(paginatedViewModel);
        }
        
        // --- TODAS LAS DEMÁS ACCIONES ESTÁN INTACTAS Y PRESERVADAS ---

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
        public async Task<IActionResult> ToggleFavorito([FromBody] ToggleGuarderiaRequest request)
        {
            var userId = _userManager.GetUserId(User);
            var favoritoExistente = await _context.FavoritosGuarderia.FirstOrDefaultAsync(f => f.GuarderiaId == request.GuarderiaId && f.UsuarioId == userId);
            bool agregado;
            if (favoritoExistente != null)
            {
                _context.FavoritosGuarderia.Remove(favoritoExistente);
                agregado = false;
            }
            else
            {
                var nuevoFavorito = new FavoritoGuarderia { GuarderiaId = request.GuarderiaId, UsuarioId = userId };
                _context.FavoritosGuarderia.Add(nuevoFavorito);
                agregado = true;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, agregado = agregado });
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
                    var urlImagenLogo = "https://raw.githubusercontent.com/gist/brudnak/aba00c9a1c92d226f68e8ad8ba1e0a40/raw/e1e4a92f6072d15014f19aa8903d24a1ac0c41a4/nyan-cat.gif"; // <-- CAMBIA ESTA URL
                    var culturaEspañol = new CultureInfo("es-ES");

                    // --- Correo para el Cliente (CON ESTILOS PRESERVADOS) ---
                    var subjectCliente = $"Confirmación de tu solicitud de cita en {guarderia.Nombre}";
                    var contentCliente = $@"
                        <div style='font-family: Arial, sans-serif; color: #3a6e4cff; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; padding: 20px;'>
                            <img src='{urlImagenLogo}' alt='Logo de Purr & Paws' style='max-width: 150px; height: auto; display: block; margin: 0 auto 20px auto;'/>
                            <h2 style='color: #4A4A4A;'>¡Hola, {model.NombreCliente}!</h2>
                            <p>Hemos recibido correctamente tu solicitud de cita para <strong style='color: #5a4fcf;'>{guarderia.Nombre}</strong>.</p>
                            <p>Estos son los detalles que registramos:</p>
                            <div style='background-color: #f7f7f7; padding: 15px; border-radius: 5px;'>
                                <ul style='list-style: none; padding: 0;'>
                                    <li><strong>Fecha:</strong> <span style='color: #e57373; font-weight: bold;'>{model.Fecha.ToString("dddd, dd 'de' MMMM 'de' yyyy", culturaEspañol)}</span></li>
                                    <li><strong>Hora:</strong> <span style='color: #e57373; font-weight: bold;'>{model.Hora.ToString(@"hh\:mm")}</span></li>
                                </ul>
                            </div>
                            <p>El personal de la guardería se pondrá en contacto contigo a la brevedad para confirmar la disponibilidad y los siguientes pasos.</p>
                            <p style='margin-top: 30px; font-size: 0.9em; color: #777;'>Atentamente,<br>El equipo de Purr & Paws</p>
                        </div>";

                    await _emailService.SendEmailAsync(model.EmailCliente, subjectCliente, contentCliente);

                    // --- Correo para el Dueño de la Guardería (CON ESTILOS PRESERVADOS) ---
                    var emailDueño = "correo.dueño.guarderia@ejemplo.com";
                    var subjectDueño = $"Nueva Solicitud de Cita: {model.NombreCliente} para el {model.Fecha:dd/MM/yyyy}";
                    var contentDueño = $@"<h1>¡Nueva Solicitud de Cita!</h1><p>Has recibido una nueva solicitud:</p><ul><li><strong>Cliente:</strong> {model.NombreCliente} ({model.EmailCliente})</li><li><strong>Fecha y Hora:</strong> {model.Fecha.ToString("dd/MM/yyyy")} a las {model.Hora.ToString(@"hh\:mm")}</li><li><strong>Mensaje:</strong> {(string.IsNullOrEmpty(model.Mensaje) ? "N/A" : model.Mensaje)}</li></ul><p>Por favor, ponte en contacto con el cliente para confirmar.</p>";

                    await _emailService.SendEmailAsync(emailDueño, subjectDueño, contentDueño);

                    return Json(new { success = true, message = "¡Solicitud enviada! Revisa tu correo para más detalles." });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500, new { success = false, message = "Hubo un error al enviar la notificación por correo." });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = "Por favor, corrige los errores.", errors = errors });
        }
    }

    public class ToggleGuarderiaRequest
    {
        public int GuarderiaId { get; set; }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.ViewModels;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using X.PagedList;

namespace PetConnect.Controllers
{
    public class ServicioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public ServicioController(ApplicationDbContext context, IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string busqueda, int? page)
        {
            ViewData["BusquedaActual"] = busqueda;

            var query = _context.Servicios
                .Include(s => s.VeterinariaDetalle) // Incluir detalles para la dirección
                .Where(s => s.Tipo == TipoServicio.Veterinaria)
                .OrderBy(s => s.Nombre)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(s => s.Nombre.ToLower().Contains(busqueda.ToLower()));
            }

            int pageSize = 6;
            var pagedServicios = await query.ToPagedListAsync(page ?? 1, pageSize);
            var viewModels = new List<ServicioViewModel>();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var favoritosIds = await _context.FavoritosServicio
                    .Where(f => f.UsuarioId == userId)
                    .Select(f => f.ServicioId)
                    .ToHashSetAsync();

                foreach (var servicio in pagedServicios)
                {
                    viewModels.Add(new ServicioViewModel { Servicio = servicio, EsFavorito = favoritosIds.Contains(servicio.Id) });
                }
            }
            else
            {
                foreach (var servicio in pagedServicios)
                {
                    viewModels.Add(new ServicioViewModel { Servicio = servicio, EsFavorito = false });
                }
            }

            var pagedViewModel = new StaticPagedList<ServicioViewModel>(viewModels, pagedServicios.GetMetaData());
            return View(pagedViewModel);
        }

        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var veterinaria = await _context.Servicios
                .Include(s => s.VeterinariaDetalle)
                .Include(s => s.Comentarios)
                    .ThenInclude(c => c.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.Veterinaria);

            if (veterinaria == null) return NotFound();

            return View(veterinaria);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(int servicioId, string textoComentario)
        {
            if (string.IsNullOrWhiteSpace(textoComentario))
                return Json(new { success = false, message = "El comentario no puede estar vacío." });

            var userId = _userManager.GetUserId(User);
            var comentario = new ComentarioServicio
            {
                Texto = textoComentario,
                FechaComentario = DateTime.UtcNow,
                ServicioId = servicioId,
                UsuarioId = userId
            };

            _context.ComentariosServicio.Add(comentario);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Comentario añadido." });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarComentario(int comentarioId)
        {
            var userId = _userManager.GetUserId(User);
            var comentario = await _context.ComentariosServicio.FindAsync(comentarioId);

            if (comentario == null) return NotFound();
            if (comentario.UsuarioId != userId) return Forbid();

            _context.ComentariosServicio.Remove(comentario);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorito([FromBody] ToggleServicioRequest request)
        {
            var userId = _userManager.GetUserId(User);

            var favoritoExistente = await _context.FavoritosServicio
                .FirstOrDefaultAsync(f => f.ServicioId == request.ServicioId && f.UsuarioId == userId);

            bool agregado;
            if (favoritoExistente != null)
            {
                _context.FavoritosServicio.Remove(favoritoExistente);
                agregado = false; // Se quitó
            }
            else
            {
                var nuevoFavorito = new FavoritoServicio
                {
                    ServicioId = request.ServicioId,
                    UsuarioId = userId
                };
                _context.FavoritosServicio.Add(nuevoFavorito);
                agregado = true; // Se añadió
            }

            await _context.SaveChangesAsync();
            
            return Json(new { success = true, agregado = agregado });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSesionDePago(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return NotFound();

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var domain = $"{Request.Scheme}://{Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 2500, Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions { Name = $"Reserva de Cita en {servicio.Nombre}", },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                // --- CORRECCIÓN APLICADA AQUÍ ---
                // Stripe adjuntará el ID de sesión a esta URL al redirigir
                SuccessUrl = $"{domain}/Servicio/ReservaConfirmada?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Servicio/Detalle/{id}",
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return Redirect(session.Url);
        }

        [HttpGet]
        public async Task<IActionResult> ReservaConfirmada([FromQuery] string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return RedirectToAction("Index");
            }
            
            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(session_id);
            
            // Comprobamos si el pago fue realmente exitoso
            if (session.PaymentStatus == "paid")
            {
                // Evitamos duplicados: comprobamos si ya guardamos este pago
                var pagoExistente = await _context.Pagos.FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
                if (pagoExistente == null)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
                    var nuevoPago = new Pago
                    {
                        UsuarioId = userId,
                        StripeSessionId = session.Id,
                        ServicioId = null, // Esto se podría mejorar pasando el ID en los metadatos de Stripe
                        Monto = (decimal)(session.AmountTotal.HasValue ? session.AmountTotal.Value / 100.0 : 0),
                        Moneda = session.Currency,
                        Descripcion = "Reserva de Cita al Veterinario",
                        Estado = "Completado",
                        FechaCreacion = DateTime.UtcNow
                    };

                    _context.Pagos.Add(nuevoPago);
                    await _context.SaveChangesAsync();
                }
            }

            // Mostramos la página de "gracias" al usuario
            return View();
        }
        
    }

    public class ToggleServicioRequest
    {
        public int ServicioId { get; set; }
    }
}
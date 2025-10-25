using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;

namespace PetConnect.Controllers
{
    public class ServicioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ServicioController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index(string busqueda, int? page)
        {
            ViewData["BusquedaActual"] = busqueda;
            
            IQueryable<Servicio> query = _context.Servicios
                .Where(s => s.Tipo == TipoServicio.Veterinaria)
                .OrderBy(s => s.Nombre) 
                .AsNoTracking();

            if (!string.IsNullOrEmpty(busqueda))
            {
                string busquedaLower = busqueda.ToLower();
                query = query.Where(s => s.Nombre.ToLower().Contains(busquedaLower));
            }
            
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            var veterinariasPaginadas = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(veterinariasPaginadas);
        }


        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var veterinaria = await _context.Servicios
                .Include(s => s.VeterinariaDetalle)
                    .ThenInclude(vd => vd.Resenas)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.Veterinaria);

            if (veterinaria == null) return NotFound();

            return View(veterinaria);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSesionDePago(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null)
            {
                return NotFound();
            }

            var secretKey = _configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = secretKey;
            
            // El resto de la lógica no cambia
            var domain = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{domain}/Servicio/ReservaConfirmada";
            var cancelUrl = $"{domain}/Servicio/Detalle/{id}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 2500, // 25.00
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Reserva de Cita en {servicio.Nombre}",
                                Description = "Confirmación de cita para consulta veterinaria.",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }

        // --- NUEVA ACCIÓN PARA LA PÁGINA DE ÉXITO ---
        public IActionResult ReservaConfirmada()
        {
            return View(); // Mostraremos una página simple de "Gracias por tu reserva"
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Controllers
{
    [Authorize] // Solo usuarios logueados pueden acceder a cualquier acción de este controlador
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PerfilController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

  
        public async Task<IActionResult> HistorialPagos()
        {
 
            var userId = _userManager.GetUserId(User);

            var pagos = await _context.Pagos
                .Where(p => p.UsuarioId == userId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();
            
            // Enviamos la lista de pagos a la vista para que los muestre.
            return View(pagos);
        }
    }
}
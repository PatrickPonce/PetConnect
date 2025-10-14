using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PetConnect.Models;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Controllers
{
    public class PetShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PetShopController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string busqueda)
        {
            IQueryable<Servicio> query = _context.Servicios
                .Where(s => s.Tipo == TipoServicio.PetShop)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(s => s.Nombre.Contains(busqueda));
                ViewData["BusquedaActual"] = busqueda;
            }

            var petShops = await query.ToListAsync();
            return View(petShops);
        }
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var petShop = await _context.Servicios
                .Include(s => s.PetShopDetalle) 
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.PetShop);

            if (petShop == null) return NotFound();

            return View(petShop);
        }
    }
}
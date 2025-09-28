using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Threading.Tasks;

namespace PetConnect.Controllers
{
    public class AdopcionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdopcionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string busqueda)
        {
            var query = _context.Servicios
                .Where(s => s.Tipo == Models.TipoServicio.Adopcion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(s => s.Nombre.Contains(busqueda) || (s.FundacionNombre != null && s.FundacionNombre.Contains(busqueda)));
            }

            var model = await query.ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var model = await _context.Servicios
                .Include(s => s.AdopcionDetalle) 
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == Models.TipoServicio.Adopcion);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}
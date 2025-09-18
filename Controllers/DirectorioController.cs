using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.ViewModels;

namespace PetConnect.Controllers;

public class DirectorioController : Controller
{
    private readonly ApplicationDbContext _context;

    public DirectorioController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Veterinaria()
    {
        var servicios = await _context.Servicios
                                    .Include(s => s.Categoria) // Ahora esto funciona
                                    .Where(s => s.Categoria.Nombre == "Veterinarias")
                                    .ToListAsync();

        return View(servicios); // Pasamos el modelo fuertemente tipado
    }
    public async Task<IActionResult> PetShop()
    {
        var servicios = await _context.Servicios
                                    .Include(s => s.Categoria) // Ahora esto funciona
                                    .Where(s => s.Categoria.Nombre == "Pet Shop")
                                    .ToListAsync();

        return View(servicios); // Pasamos el modelo fuertemente tipado
    }

    // --- NUEVO MÉTODO PARA LA PÁGINA DE DETALLE ---
    public async Task<IActionResult> Detalle(int id)
    {
        var servicio = await _context.Servicios
            .Include(s => s.Categoria)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servicio == null)
        {
            return NotFound();
        }

        var resenas = await (
            from resena in _context.Resenas
            where resena.ServicioId == id
            join user in _context.Users on resena.UsuarioId.ToString() equals user.Id
            orderby resena.FechaCreacion descending
            select new ResenaViewModel
            {
                Contenido = resena.Contenido,
                Puntuacion = resena.Puntuacion,
                FechaCreacion = resena.FechaCreacion,
                NombreUsuario = user.NombreCompleto,
                OcupacionUsuario = "Desarrollador/a" // Dato de ejemplo, puedes añadirlo a tu ApplicationUser
            }
        ).ToListAsync();

        var viewModel = new ServicioDetalleViewModel
        {
            Servicio = servicio,
            Resenas = resenas
        };

        return View(viewModel);
    }
    
    public async Task<IActionResult> Guarderia()
    {
        var servicios = await _context.Servicios
                                    .Include(s => s.Categoria)
                                    .Where(s => s.Categoria.Nombre == "Guardería")
                                    .ToListAsync();
        
        return View(servicios);
    }


}
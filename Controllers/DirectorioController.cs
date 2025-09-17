using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;

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
                                .ToListAsync();
    
    return View(servicios); // Pasamos el modelo fuertemente tipado
}
}
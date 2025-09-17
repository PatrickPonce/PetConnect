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
    var serviciosConCategoria = await (
        from servicio in _context.Servicios
        join categoria in _context.Categorias on servicio.CategoriaId equals categoria.Id
        select new 
        {
            Id = servicio.Id,
            Nombre = servicio.Nombre,
            ImagenUrl = servicio.ImagenPrincipalUrl,
            Direccion = servicio.Direccion,
            NombreCategoria = categoria.Nombre 
        }
    ).ToListAsync();

    ViewBag.Servicios = serviciosConCategoria;
    
    return View();
}
}
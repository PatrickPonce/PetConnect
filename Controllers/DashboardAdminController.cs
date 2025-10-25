// Controllers/DashboardAdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models; // Necesario para TipoServicio y Noticia
using PetConnect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")] // Solo Admins
public class DashboardAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardAdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel();

        // --- 1. Datos para Gráfico de Servicios ---
        viewModel.ConteoServiciosPorTipo.Add("Veterinarias", await _context.Servicios.CountAsync(s => s.Tipo == TipoServicio.Veterinaria));
        viewModel.ConteoServiciosPorTipo.Add("Pet Friendly", await _context.LugaresPetFriendly.CountAsync()); // Contamos la tabla separada
        viewModel.ConteoServiciosPorTipo.Add("Guardería", await _context.Guarderias.CountAsync()); // Contamos la tabla separada
        viewModel.ConteoServiciosPorTipo.Add("Adopción", await _context.Servicios.CountAsync(s => s.Tipo == TipoServicio.Adopcion));
        viewModel.ConteoServiciosPorTipo.Add("Pet Shop", await _context.Servicios.CountAsync(s => s.Tipo == TipoServicio.PetShop));

        // --- 2. Datos para Gráfico de Usuarios Registrados ---
        // IMPORTANTE: IdentityUser no guarda fecha de registro por defecto.
        // Usaremos datos de EJEMPLO. Para datos reales, necesitarías añadir
        // una propiedad de FechaRegistro al crear el usuario.
        viewModel.EtiquetasTiempoUsuarios = new List<string> { "2020", "2021", "2022", "2023", "2024", "2025" };
        viewModel.DatosConteoUsuarios = new List<int> { 10, 65, 30, 150, 50, 90 }; // Datos inventados

        // --- 3. Datos para Gráfico de Noticias Publicadas ---
        var noticiasPorAno = await _context.Noticias
                                    .GroupBy(n => n.FechaPublicacion.Year) // Agrupamos por año
                                    .Select(g => new { Ano = g.Key, Count = g.Count() })
                                    .OrderBy(x => x.Ano)
                                    .ToListAsync();

        viewModel.EtiquetasTiempoNoticias = noticiasPorAno.Select(x => x.Ano.ToString()).ToList();
        viewModel.DatosConteoNoticias = noticiasPorAno.Select(x => x.Count).ToList();

         // Aseguramos que haya datos de ejemplo si no hay noticias aún
         if (!viewModel.EtiquetasTiempoNoticias.Any()) {
             viewModel.EtiquetasTiempoNoticias = new List<string> { "2024", "2025" };
             viewModel.DatosConteoNoticias = new List<int> { 0, 0 };
         }


        return View(viewModel);
    }
}
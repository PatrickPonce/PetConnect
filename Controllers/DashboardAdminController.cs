using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using PetConnect.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

[Authorize(Roles = "Admin")]
public class DashboardAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager; // Para contar usuarios

    public DashboardAdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();

        // 1. KPIs
        model.TotalUsuarios = await _userManager.Users.CountAsync();
        model.TotalServicios = await _context.Servicios.CountAsync();
        model.TotalNoticias = await _context.Noticias.CountAsync();
        // (Asumiendo que tienes una tabla unificada de reseñas o sumas varias)
        model.TotalResenas = await _context.Resenas.CountAsync() 
                           + await _context.Comentarios.CountAsync(); // Ejemplo combinado

        // 2. Datos para Gráficos (Servicios por Tipo)
        var serviciosPorTipo = await _context.Servicios
            .GroupBy(s => s.Tipo)
            .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        model.CantidadVeterinarias = serviciosPorTipo.FirstOrDefault(s => s.Tipo == TipoServicio.Veterinaria)?.Cantidad ?? 0;
        model.CantidadPetShops = serviciosPorTipo.FirstOrDefault(s => s.Tipo == TipoServicio.PetShop)?.Cantidad ?? 0;
        model.CantidadLugares = serviciosPorTipo.FirstOrDefault(s => s.Tipo == TipoServicio.LugarPetFriendly)?.Cantidad ?? 0;
        model.CantidadGuarderias = serviciosPorTipo.FirstOrDefault(s => s.Tipo == TipoServicio.Guarderia)?.Cantidad ?? 0;
        model.CantidadAdopcion = serviciosPorTipo.FirstOrDefault(s => s.Tipo == TipoServicio.Adopcion)?.Cantidad ?? 0;

        // 3. Datos para Gráfico de Usuarios (Simulado por ahora, ya que Identity no guarda fecha de registro fácilmente por defecto)
        // Para hacerlo real, necesitarías añadir un campo 'FechaRegistro' a tu usuario personalizado.
        model.UsuariosPorMes = new int[] { 5, 8, 12, 15, 22, 30, 45, 50, 55, 60, 75, 80 }; // Datos de ejemplo

        // 4. Actividad Reciente (Simulada con datos reales mezclados)
        // Aquí podrías hacer una unión de las últimas noticias, comentarios, nuevos usuarios, etc.
        var ultimasNoticias = await _context.Noticias.OrderByDescending(n => n.FechaPublicacion).Take(3).ToListAsync();
        foreach (var noticia in ultimasNoticias)
        {
            model.UltimasActividades.Add(new ActividadReciente
            {
                Descripcion = $"Nueva noticia publicada: {noticia.Titulo}",
                Fecha = noticia.FechaPublicacion.ToString("dd/MM/yyyy HH:mm"),
                Icono = "article",
                ColorIcono = "#4CAF50" // Verde
            });
        }
        // (Añade aquí más actividades reales de otras tablas si quieres)

        return View(model);
    }
}
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

    // --- MÉTODO INDEX TOTALMENTE ACTUALIZADO ---
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();

        // --- 1. KPIs (Indicadores Clave) ---
        
        // Contamos usuarios y noticias (esto estaba bien)
        model.TotalUsuarios = await _userManager.Users.CountAsync();
        model.TotalNoticias = await _context.Noticias.CountAsync();

        // CORRECCIÓN: Contamos el total de reseñas de TODAS las tablas de reseñas/comentarios
        int totalResenasProductos = await _context.ResenasProducto.CountAsync();
        int totalComentariosNoticias = await _context.Comentarios.CountAsync();
        int totalComentariosLugares = await _context.ComentariosLugar.CountAsync();
        int totalComentariosGuarderias = await _context.ComentariosGuarderia.CountAsync();
        int totalComentariosServicios = await _context.ComentariosServicio.CountAsync();
        model.TotalResenas = totalResenasProductos + totalComentariosNoticias + totalComentariosLugares + totalComentariosGuarderias + totalComentariosServicios;


        // --- 2. Datos para Gráfico y KPI de Servicios (LA CORRECCIÓN PRINCIPAL) ---
        
        // Obtenemos los recuentos de cada tabla independiente
        model.CantidadVeterinarias = await _context.Servicios.CountAsync(s => s.Tipo == TipoServicio.Veterinaria);
        model.CantidadAdopcion = await _context.Servicios.CountAsync(s => s.Tipo == TipoServicio.Adopcion);
        model.CantidadPetShops = await _context.ProductosPetShop.CountAsync(); // <-- Lee de ProductosPetShop
        model.CantidadLugares = await _context.LugaresPetFriendly.CountAsync(); // <-- Lee de LugaresPetFriendly
        model.CantidadGuarderias = await _context.Guarderias.CountAsync(); // <-- Lee de Guarderias

        // CORRECCIÓN: El KPI "TotalServicios" ahora es la suma de todas las categorías
        model.TotalServicios = model.CantidadVeterinarias + model.CantidadAdopcion + model.CantidadPetShops + model.CantidadLugares + model.CantidadGuarderias;


        // --- 3. Datos para Gráfico de Usuarios (Sin cambios) ---
        // (Esto sigue siendo un ejemplo, necesitarías una lógica de BD real para agrupar por mes)
        model.UsuariosPorMes = new int[] { 5, 8, 12, 15, 22, 30, 45, 50, 55, 60, 75, 80 };


        // --- 4. Actividad Reciente (Sin cambios) ---
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

        return View(model);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Models; // Asegúrate que el namespace de tus modelos es correcto
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PetConnect.Data;

// El nombre del controlador es "Lugares", por lo que en las URLs y Tag Helpers se usará "Lugares"
public class LugaresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    // Inyectamos el DbContext y el UserManager para acceder a la BD y a los usuarios
    public LugaresController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ACCIÓN PARA MOSTRAR LA LISTA DE LUGARES
    // Se activa con la URL: /Lugares/Index o simplemente /Lugares
    public async Task<IActionResult> Index()
    {
        // Obtiene todos los lugares de la base de datos y los manda a la vista
        var lugares = await _context.LugaresPetFriendly.ToListAsync();
        return View(lugares);
    }

    // ACCIÓN PARA MOSTRAR LOS DETALLES DE UN LUGAR ESPECÍFICO
    // Se activa con una URL como: /Lugares/Detalle/1
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null)
        {
            return NotFound(); // Si no se proporciona un ID, devuelve error 404
        }

        // Busca el lugar por su ID.
        // Usamos Include y ThenInclude para cargar los datos relacionados (comentarios y el usuario de cada comentario).
        // Si no hacemos esto, Model.Comentarios estaría vacío en la vista.
        var lugar = await _context.LugaresPetFriendly
            .Include(l => l.Comentarios)
                .ThenInclude(c => c.Usuario) 
            .FirstOrDefaultAsync(m => m.Id == id);

        if (lugar == null)
        {
            return NotFound(); // Si no se encuentra el lugar, devuelve error 404
        }

        return View(lugar); // Manda el objeto 'lugar' a la vista Detalle.cshtml
    }

    // ACCIÓN PARA AÑADIR UN NUEVO COMENTARIO (SE LLAMA DESDE JAVASCRIPT)
    [HttpPost] // Solo responde a peticiones POST
    [Authorize] // Requiere que el usuario haya iniciado sesión
    [ValidateAntiForgeryToken] // Medida de seguridad
    public async Task<IActionResult> AgregarComentario(int lugarId, string textoComentario)
    {
        if (string.IsNullOrWhiteSpace(textoComentario))
        {
            return Json(new { success = false, message = "El comentario no puede estar vacío." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtiene el ID del usuario actual
        var usuario = await _userManager.FindByIdAsync(userId);

        var comentario = new ComentarioLugar
        {
            Texto = textoComentario,
            FechaComentario = DateTime.UtcNow,
            LugarPetFriendlyId = lugarId,
            UsuarioId = userId
        };

        _context.ComentariosLugar.Add(comentario);
        await _context.SaveChangesAsync(); // Guarda el comentario en la base de datos

        // Devuelve una respuesta JSON al JavaScript que lo llamó
        return Json(new { 
            success = true, 
            message = "Comentario añadido.", 
            autor = usuario.UserName, 
            texto = comentario.Texto, 
            fecha = comentario.FechaComentario.ToString("g") 
        });
    }

    // ACCIÓN PARA AÑADIR/QUITAR DE FAVORITOS (SE LLAMA DESDE JAVASCRIPT)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorito(int lugarId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var favoritoExistente = await _context.FavoritosLugar
            .FirstOrDefaultAsync(f => f.LugarPetFriendlyId == lugarId && f.UsuarioId == userId);

        bool agregado;
        if (favoritoExistente != null)
        {
            // Si ya existe, lo eliminamos
            _context.FavoritosLugar.Remove(favoritoExistente);
            agregado = false;
        }
        else
        {
            // Si no existe, lo creamos y guardamos
            var nuevoFavorito = new FavoritoLugar
            {
                LugarPetFriendlyId = lugarId,
                UsuarioId = userId
            };
            _context.FavoritosLugar.Add(nuevoFavorito);
            agregado = true;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, agregado = agregado });
    }
}
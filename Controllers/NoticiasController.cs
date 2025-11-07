using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; 
using PetConnect.Data; 
using PetConnect.Models; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Claims;
using PetConnect.Claims;
using PetConnect.Services;
using Microsoft.AspNetCore.SignalR;
using PetConnect.Hubs;
using System.Text.RegularExpressions;
public class NoticiasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly PerspectiveService _perspectiveService;
    private readonly IHubContext<ComentarioHub> _hubContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly GeminiService _geminiService;
    private readonly MlNetPredictionService _mlNetPredictionService;

    public NoticiasController(ApplicationDbContext context,
                              UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager, PerspectiveService perspectiveService,
                              IHubContext<ComentarioHub> hubContext, IHttpClientFactory httpClientFactory, IConfiguration configuration,
                              GeminiService geminiService,MlNetPredictionService mlNetPredictionService) 
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _perspectiveService = perspectiveService;
        _hubContext = hubContext;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _geminiService = geminiService;
        _mlNetPredictionService = mlNetPredictionService;
    }

 
    public async Task<IActionResult> Index()
    {
        var noticias = await _context.Noticias
                                      .OrderByDescending(n => n.EsFijada) 
                                    .ThenByDescending(n => n.FechaPublicacion) 
                                      .ToListAsync();

        var favoritosDelUsuario = new HashSet<int>();
        if (_signInManager.IsSignedIn(User))
        {
            var userId = _userManager.GetUserId(User);
      
            var ids = await _context.Favoritos
                                    .Where(f => f.UsuarioId == userId)
                                    .Select(f => f.NoticiaId)
                                    .ToListAsync();
            favoritosDelUsuario = new HashSet<int>(ids);
        }

        ViewData["FavoritosDelUsuario"] = favoritosDelUsuario;

        foreach (var noticia in noticias)
        {
            string contenidoLimpio = Regex.Replace(noticia.Contenido ?? string.Empty, "<.*?>", String.Empty);
            
            if (contenidoLimpio.Length > 100)
            {
                noticia.Contenido = contenidoLimpio.Substring(0, 100) + "...";
            } 
            else
            {
                noticia.Contenido = contenidoLimpio;
            }
        }

        return View(noticias);
    }
    
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var noticia = await _context.Noticias
                                    .Include(n => n.Comentarios) 
                                    .FirstOrDefaultAsync(n => n.Id == id);

        if (noticia == null)
        {
            return NotFound();
        }
        noticia.Vistas++;
        _context.Update(noticia);
        await _context.SaveChangesAsync();

        bool esFavorito = false;
        if (_signInManager.IsSignedIn(User))
        {
            var userId = _userManager.GetUserId(User);
            esFavorito = await _context.Favoritos
                .AnyAsync(f => f.NoticiaId == noticia.Id && f.UsuarioId == userId);
        }

        ViewData["EsFavorito"] = esFavorito; 
        Console.WriteLine($"DEBUG CONTROLLER Detalle: User.Identity.IsAuthenticated = {User.Identity.IsAuthenticated}");

        return View(noticia);
    }

    [Authorize] 
    public async Task<IActionResult> Favoritos()
    {
        var userId = _userManager.GetUserId(User);

       

        var noticiasFavoritas = await _context.Favoritos
            .Where(f => f.UsuarioId == userId)  
            .OrderByDescending(f => f.FechaAgregado) 
            .Select(f => f.Noticia) 
            .ToListAsync();
        
        var favoritosDelUsuario = new HashSet<int>(noticiasFavoritas.Select(n => n.Id));
        ViewData["FavoritosDelUsuario"] = favoritosDelUsuario;
      
        foreach (var noticia in noticiasFavoritas)
        {
             string contenidoLimpio = Regex.Replace(noticia.Contenido ?? string.Empty, "<.*?>", String.Empty);
             if (contenidoLimpio.Length > 100) { noticia.Contenido = contenidoLimpio.Substring(0, 100) + "..."; } 
             else { noticia.Contenido = contenidoLimpio; }
        }
        
        return View(noticiasFavoritas);
    }

    [HttpPost]
    [Authorize]
    // Cambia el parámetro para que acepte un objeto
    public async Task<IActionResult> ToggleFavorito([FromBody] ToggleFavoritoRequest request) 
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        // Usa request.NoticiaId en lugar de noticiaId
        var favoritoExistente = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.NoticiaId == request.NoticiaId && f.UsuarioId == userId);

        bool esFavoritoAhora;

        if (favoritoExistente != null)
        {
            _context.Favoritos.Remove(favoritoExistente);
            esFavoritoAhora = false;
        }
        else
        {
            var nuevoFavorito = new Favorito
            {
                NoticiaId = request.NoticiaId, // <-- Usa request.NoticiaId
                UsuarioId = userId,
                FechaAgregado = DateTime.UtcNow
            };
            _context.Favoritos.Add(nuevoFavorito);
            esFavoritoAhora = true;
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, esFavorito = esFavoritoAhora });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EliminarFavoritos([FromBody] List<int> noticiaIds)
    {
        if (noticiaIds == null || !noticiaIds.Any())
        {
            return Json(new { success = false, message = "No se seleccionaron noticias." });
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            
            return Unauthorized(new { success = false, message = "Usuario no autorizado." });
        }

        
        var favoritosAEliminar = await _context.Favoritos
            .Where(f => f.UsuarioId == userId && noticiaIds.Contains(f.NoticiaId))
            .ToListAsync();

        
        if (favoritosAEliminar.Any())
        {
            _context.Favoritos.RemoveRange(favoritosAEliminar);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
            
                Console.WriteLine($"Advertencia de concurrencia al eliminar favoritos: {ex.Message}");
                
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error al guardar cambios al eliminar favoritos: {ex.Message}");
               
                return Json(new { success = false, message = "Ocurrió un error inesperado al eliminar." });
            }
        }
        return Json(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int noticiaId, string textoComentario)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Usuario no encontrado." });
        }
        bool esToxico = await _perspectiveService.EsComentarioToxico(textoComentario);
        if (esToxico)
        {
            // Si la IA lo detecta, lo rechaza
            return Json(new { success = false, message = "Tu comentario infringe las normas de la comunidad y no ha sido publicado." });
        }

        // 1. Obtener la Foto de Perfil (Usando la lógica de Claims)
        var claims = await _userManager.GetClaimsAsync(user);
        var fotoClaim = claims.FirstOrDefault(c => c.Type == PetConnectClaimTypes.ProfilePictureUrl);
        var fotoUrl = fotoClaim?.Value ?? "/images/avatars/default.png"; // <-- Tu ruta por defecto

        // 2. Obtener el Nombre de Usuario
        var autorNombre = user.UserName ?? "Anónimo";
        if (autorNombre.Contains("@"))
        {
            autorNombre = autorNombre.Split('@')[0];
        }
        
        // 3. Validar texto
        if (string.IsNullOrWhiteSpace(textoComentario) || textoComentario.Length < 3 || textoComentario.Length > 500)
        {
            return Json(new { success = false, message = "El comentario debe tener entre 3 y 500 caracteres." });
        }

        var nuevoComentario = new Comentario
        {
            NoticiaId = noticiaId,
            Texto = textoComentario,
            Autor = autorNombre, 
            FechaComentario = DateTime.UtcNow,
            AutorId = user.Id,
            AutorFotoUrl = fotoUrl
        };

        _context.Comentarios.Add(nuevoComentario);
        await _context.SaveChangesAsync();

        // --- INICIO DE LA MODIFICACIÓN DE SIGNALR ---

        // 1. Prepara los datos que se enviarán (deben coincidir con el JSON de retorno)
        var dataParaCliente = new
        {
            success = true,
            id = nuevoComentario.Id,
            autor = nuevoComentario.Autor,
            texto = nuevoComentario.Texto,
            fechaISO = nuevoComentario.FechaComentario.ToString("o"),
            fotoUrl = nuevoComentario.AutorFotoUrl 
        };

        // 2. Define el nombre del grupo para esta noticia
        string grupoNoticia = $"noticia-{noticiaId}";

        // 3. Envía el mensaje "RecibirComentario" a todos en ese grupo
        await _hubContext.Clients.Group(grupoNoticia).SendAsync("RecibirComentario", dataParaCliente);
        
        // --- FIN DE LA MODIFICACIÓN DE SIGNALR ---

        // 4. Devuelve la respuesta solo al cliente que envió el comentario
        return Json(dataParaCliente);
    }
    [HttpPost]
    [Authorize] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarComentario(int comentarioId)
    {
        if (comentarioId <= 0)
        {
            return Json(new { success = false, message = "ID de comentario inválido." });
        }

        var userId = _userManager.GetUserId(User); // <-- Obtener el ID del usuario actual
        var comentario = await _context.Comentarios.FindAsync(comentarioId);

        if (comentario == null)
        {
            return Json(new { success = false, message = "Comentario no encontrado." });
        }

        // --- COMPROBACIÓN DE PERMISO CORRECTA ---
        if (comentario.AutorId != userId)
        {
            // El usuario actual no es el autor del comentario
            return Json(new { success = false, message = "No tienes permiso para eliminar este comentario." });
        }
        
        int noticiaId = comentario.NoticiaId; 

        _context.Comentarios.Remove(comentario);
        await _context.SaveChangesAsync();

        string grupoNoticia = $"noticia-{noticiaId}";
        string eventoSignalR = "ComentarioEliminado";

        await _hubContext.Clients.Group(grupoNoticia).SendAsync(eventoSignalR, comentarioId);

        return Json(new { success = true });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarComentario(int comentarioId, string nuevoTexto)
    {
        if (comentarioId <= 0 || string.IsNullOrWhiteSpace(nuevoTexto) || nuevoTexto.Length < 3 || nuevoTexto.Length > 500)
        {
            return Json(new { success = false, message = "El comentario no es válido (3-500 caracteres)." });
        }

        var userId = _userManager.GetUserId(User); // <-- Obtener el ID del usuario actual
        var comentario = await _context.Comentarios.FindAsync(comentarioId);

        if (comentario == null)
        {
            return Json(new { success = false, message = "Comentario no encontrado." });
        }

        // --- COMPROBACIÓN DE PERMISO CORRECTA ---
        if (comentario.AutorId != userId)
        {
            return Json(new { success = false, message = "No tienes permiso para editar este comentario." });
        }

        TimeSpan diferencia = DateTime.UtcNow - comentario.FechaComentario.ToUniversalTime();
        if (diferencia.TotalMinutes > 15)
        {
            return Json(new { success = false, message = "Ya no puedes editar este comentario (límite de 15 min)." });
        }

        comentario.Texto = nuevoTexto;
        comentario.FechaComentario = DateTime.UtcNow; // Actualizar la fecha al editar

        _context.Comentarios.Update(comentario);
        await _context.SaveChangesAsync();

        // --- JSON DE RESPUESTA CORREGIDO (SIN EL ERROR) ---
        var dataParaCliente = new
        {
            success = true,
            id = comentario.Id, // El ID es crucial para que el JS sepa qué comentario actualizar
            texto = comentario.Texto,
            fechaISO = comentario.FechaComentario.ToString("o")
        };

        // 2. Define el grupo y el nombre del evento
        string grupoNoticia = $"noticia-{comentario.NoticiaId}";
        string eventoSignalR = "ComentarioEditado";

        // 3. Envía el mensaje a todos en el grupo
        await _hubContext.Clients.Group(grupoNoticia).SendAsync(eventoSignalR, dataParaCliente);

        // 4. Devuelve la respuesta al cliente que hizo la edición
        return Json(dataParaCliente);
    }
    // Añade estos métodos DENTRO de tu clase NoticiasController

// 1. GET: /Noticias/Administrador (Muestra la lista de noticias como en image_3cc995.png)
    [Authorize(Roles = "Admin")] // ¡Importante! Solo los admins pueden ver esto
    public async Task<IActionResult> Administrador()
    {
        var noticias = await _context.Noticias
                                    .OrderByDescending(n => n.EsFijada) 
                                    .ThenByDescending(n => n.FechaPublicacion) 
                                    .ToListAsync();
        // Esta acción usará una nueva vista: Views/Noticias/Administrador.cshtml
        return View(noticias);
    }

    // 2. GET: /Noticias/Crear (Muestra el formulario para crear una noticia nueva)

    [Authorize(Roles = "Admin")]
    public IActionResult Crear(string titulo, string urlImagen, string contenido) // <-- AÑADE ESTOS PARÁMETROS
    {
        // 1. Crea un modelo "borrador" con los datos de la URL
        var model = new Noticia
        {
            Titulo = titulo,
            UrlImagen = urlImagen,
            Contenido = contenido
        };

        // 2. Pasa el borrador a la vista
        // Los campos (Titulo, UrlImagen, Contenido) en tu
        // formulario .cshtml se auto-rellenarán mágicamente.
        return View(model);
    }
    // EN: NoticiasController.cs

// 3. POST: /Noticias/Crear (Recibe los datos del formulario y guarda la noticia)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    // Usamos [Bind] por seguridad, para que solo acepte los campos del formulario
    public async Task<IActionResult> Crear([Bind("Titulo,UrlImagen,Contenido,Tags")] Noticia noticia)
    {
        // Verifica si el Título y Contenido (requeridos) están presentes
        if (ModelState.IsValid)
        {
            // Si todo es válido:
            // 1. Pone la fecha actual
            noticia.FechaPublicacion = DateTime.UtcNow; 
            
            // 2. Lo añade a la base de datos
            _context.Add(noticia);
            
            // 3. Guarda los cambios
            await _context.SaveChangesAsync();
            
            // 4. Te redirige a la página de "Administrador"
            return RedirectToAction(nameof(Administrador)); 
        }
        
        // Si algo falló (ej. el Contenido estaba vacío),
        // vuelve a mostrar la página "Crear" con los datos que el usuario escribió
        // y mostrará los mensajes de error.
        return View(noticia);
    }

    // 4. GET: /Noticias/Editar/5 (Muestra el formulario de edición como en image_3cc977.png)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia == null)
        {
            return NotFound();
        }
        // Usará la vista: Views/Noticias/Editar.cshtml
        return View(noticia);
    }

// 5. POST: /Noticias/Editar/5 (Recibe los datos del formulario de edición y guarda los cambios)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Titulo,Contenido,UrlImagen,FechaPublicacion,Tags")] Noticia noticia)
    {
        if (id != noticia.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                noticia.FechaPublicacion = noticia.FechaPublicacion.ToUniversalTime();
                _context.Update(noticia);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Noticias.Any(e => e.Id == noticia.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Administrador));
        }
        return View(noticia);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var noticia = await _context.Noticias.FirstOrDefaultAsync(m => m.Id == id);
        if (noticia == null)
        {
            return NotFound();
        }
        // Usará la vista: Views/Noticias/Eliminar.cshtml
        return View(noticia);
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia != null)
        {
            _context.Noticias.Remove(noticia);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Administrador));
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    // Cambiamos la ruta para que acepte el ID (ej. /Noticias/AlternarFijado/5)
    [Route("Noticias/AlternarFijado/{id:int}")]
    public async Task<IActionResult> AlternarFijado(int id)
    {
        var noticia = await _context.Noticias.FindAsync(id);
        if (noticia == null)
        {
            return Json(new { success = false, message = "Noticia no encontrada." });
        }

        // Invierte el estado actual (si era true, lo vuelve false)
        noticia.EsFijada = !noticia.EsFijada;
        _context.Update(noticia);
        await _context.SaveChangesAsync();

        // Devuelve el nuevo estado para que el JavaScript actualice la vista
        return Json(new { success = true, esFijada = noticia.EsFijada });
    }
    // Pega esto dentro de tu clase NoticiasController

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DetalleAdmin(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var noticia = await _context.Noticias
                                    .Include(n => n.Comentarios) // Incluye los comentarios para contarlos
                                    .Include(n => n.Favoritos)   // Incluye los favoritos para contarlos
                                    .FirstOrDefaultAsync(m => m.Id == id);

        if (noticia == null)
        {
            return NotFound();
        }

        // (La vista 'DetalleAdmin.cshtml' se encargará de
        // inyectar el 'ConfiguracionSitioService' para obtener los colores)

        return View(noticia); // Devuelve la nueva vista 'DetalleAdmin.cshtml'
    }
    public class ToggleFavoritoRequest
    {
        public int NoticiaId { get; set; }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> BuscarNoticiasGNews(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "El término de búsqueda no puede estar vacío." });
        }

        // 1. Obtiene la clave de API de appsettings.json
        string apiKey = _configuration["GNews:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return StatusCode(500, new { message = "Clave de API de GNews no configurada en el servidor." });
        }

        // 2. Prepara la llamada a la API
        string url = $"https://gnews.io/api/v4/search?q={Uri.EscapeDataString(q)}&lang=es&max=10&token={apiKey}";
        var cliente = _httpClientFactory.CreateClient();

        try
        {
            // 3. Llama a la API y deserializa el JSON
            var respuesta = await cliente.GetFromJsonAsync<GNewsResponse>(url);

            if (respuesta == null || respuesta.Articles == null)
            {
                return NotFound(new { message = "No se encontraron artículos." });
            }

            // 4. Devuelve los artículos como JSON
            return Json(respuesta.Articles);
        }
        catch (HttpRequestException ex)
        {
            // Captura errores si GNews falla (ej. clave incorrecta, límite excedido)
            return StatusCode(502, new { message = $"Error al llamar a la API de GNews: {ex.Message}" });
        }
    }
    // Pega este método dentro de tu clase NoticiasController

    [Authorize(Roles = "Admin")] // Asegura que solo los Admins puedan ver esta página
    [HttpGet]
    public IActionResult Importar()
    {
        // Esta simple línea busca el archivo "Importar.cshtml" 
        // en la carpeta "Views/Noticias/" y lo muestra como una página.
        return View();
    }
    // EN: NoticiasController.cs

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> BuscarFotosUnsplash(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "El término de búsqueda no puede estar vacío." });
        }

        // 1. Obtiene la clave de Unsplash
        string accessKey = _configuration["Unsplash:AccessKey"];
        if (string.IsNullOrEmpty(accessKey))
        {
            return StatusCode(500, new { message = "Clave de API de Unsplash no configurada." });
        }

        // 2. Prepara la llamada
        // orientation=landscape (solo fotos horizontales)
        // per_page=12 (trae 12 fotos)
        string url = $"https://api.unsplash.com/search/photos?query={Uri.EscapeDataString(q)}&orientation=landscape&per_page=12";

        var cliente = _httpClientFactory.CreateClient();

        // 3. Unsplash REQUIERE que te identifiques en la cabecera (Header)
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", accessKey);

        try
        {
            // 4. Llama a la API
            var respuesta = await cliente.GetFromJsonAsync<UnsplashSearchResponse>(url);

            if (respuesta == null || respuesta.Results == null)
            {
                return NotFound(new { message = "No se encontraron fotos." });
            }

            // 5. Devuelve solo los datos que necesitamos (para no enviar tanta info)
            var fotosSimples = respuesta.Results.Select(foto => new
            {
                id = foto.Id,
                urlPequena = foto.Urls.Small, // Para la miniatura
                urlRegular = foto.Urls.Regular, // Para la noticia
                autor = foto.User.Name
            }).ToList();

            return Json(fotosSimples);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = $"Error al llamar a la API de Unsplash: {ex.Message}" });
        }
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerarBorrador([FromForm] string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            return Json(new { success = false, message = "El título no puede estar vacío." });
        }

        // Llama al servicio de IA
        string borradorHtml = await _geminiService.GenerarBorradorDeNoticia(titulo);

        return Json(new { success = true, borrador = borradorHtml });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> VerModelos()
    {
        // 1. Obtiene la clave de API (la misma que usa GenerarArticulo)
        string apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            // Si no la encuentra, te avisa.
            return Content("Error: Clave de API de Gemini no configurada en secrets.json.");
        }
        // ✅ URL CORRECTA PARA LISTAR MODELOS
        string url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
        var cliente = _httpClientFactory.CreateClient();

        try
        {
            // 3. Llama a la API (es una petición GET)
            var httpResponse = await cliente.GetAsync(url);
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {

                return Content($"ERROR AL LISTAR MODELOS: {jsonResponse}");
            }

            // 4. Si todo sale bien, te mostrará la lista de modelos como JSON
            return Content(jsonResponse, "application/json");
        }
        catch (Exception ex)
        {
            return Content($"Error de C#: {ex.Message}");
        }
    }
    // Pega este método NUEVO en NoticiasController.cs
// ¡Asegúrate de tener "using System.Text.RegularExpressions;" al inicio de tu archivo!

    // En NoticiasController.cs

    // Este código ya está correcto. Asegúrate de tenerlo en tu controlador.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SugerirEtiquetas([FromForm] string titulo, [FromForm] string contenido)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(contenido))
        {
            return Json(new { success = false, message = "El título y el contenido son necesarios." });
        }

        // 1. Limpiamos el HTML para el análisis de ML.NET y Gemini
        string contenidoLimpio = Regex.Replace(contenido, "<.*?>", String.Empty);
        
        if (string.IsNullOrWhiteSpace(contenidoLimpio) || contenidoLimpio.Length < 50)
        {
            return Json(new { success = false, message = "El contenido es muy corto para generar etiquetas." });
        }

        try
        {
            // 2. ✅ USAR ML.NET: Predecir la categoría principal de la noticia.
            string etiquetaPrincipal = _mlNetPredictionService.PredecirCategoria(titulo, contenidoLimpio);

            if (etiquetaPrincipal == "Modelo no disponible")
            {
                return Json(new { success = false, message = "El modelo de clasificación (ML.NET) no ha sido cargado." });
            }
            
            // 3. Usar Gemini: Enviamos el TÍTULO y la ETIQUETA PREDECIDA a Gemini para que genere la lista final.
            string etiquetasRespuesta = await _geminiService.GenerarEtiquetas(titulo, etiquetaPrincipal); 

            if (etiquetasRespuesta.StartsWith("Error:"))
            {
                return Json(new { success = false, message = etiquetasRespuesta });
            }

            // 4. ¡Éxito! Devolvemos la lista de etiquetas
            return Json(new { success = true, etiquetas = etiquetasRespuesta });
        }
        catch (Exception ex)
        {
            // Error general del servidor
            return Json(new { success = false, message = $"Error interno del servidor: {ex.Message}" });
        }
    }
        
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using PetConnect.ViewModels; // <-- AÑADE ESTE USING

namespace PetConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Veterinarias")]
    public class VeterinariasAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public VeterinariasAdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // ... (El método SubirImagen se mantiene igual) ...
        private async Task<string> SubirImagen(IFormFile archivo)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(archivo.FileName);
            string extension = Path.GetExtension(archivo.FileName);
            string nuevoNombre = $"{fileName}_{DateTime.Now.Ticks}{extension}";
            string pathDestino = Path.Combine(wwwRootPath, "images", "servicios");

            if (!Directory.Exists(pathDestino)) Directory.CreateDirectory(pathDestino);

            using (var fileStream = new FileStream(Path.Combine(pathDestino, nuevoNombre), FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }

            return $"/images/servicios/{nuevoNombre}";
        }

        // ... (El método Index se mantiene igual) ...
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var query = _context.Servicios
                .Where(s => s.Tipo == TipoServicio.Veterinaria)
                .OrderBy(p => p.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Nombre.Contains(searchString));
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedData = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedData); // -> Sigue enviando List<Servicio> a la vista Index
        }


        // --- ACCIÓN CREATE (MODIFICADA) ---
        [Route("Crear")]
        public IActionResult Create()
        {
            // Ahora usamos el ViewModel
            var model = new VeterinariaAdminViewModel();
            return View(model);
        }

        [HttpPost]
        [Route("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VeterinariaAdminViewModel viewModel, IFormFile? imagenArchivo)
        {
            if (ModelState.IsValid) // <-- Ahora la validación es simple y funcionará
            {
                // 1. Mapeamos el ViewModel a los modelos de BD
                var servicio = new Servicio
                {
                    Tipo = TipoServicio.Veterinaria,
                    Nombre = viewModel.Nombre,
                    DescripcionCorta = viewModel.DescripcionCorta,
                    VeterinariaDetalle = new VeterinariaDetalle
                    {
                        Direccion = viewModel.Direccion,
                        Telefono = viewModel.Telefono,
                        Horario = viewModel.Horario,
                        TelefonoSecundario = viewModel.TelefonoSecundario,
                        DescripcionLarga = viewModel.DescripcionLarga
                    }
                };
                
                if (imagenArchivo != null)
                {
                    servicio.ImagenPrincipalUrl = await SubirImagen(imagenArchivo);
                }

                _context.Add(servicio);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Veterinaria creada con éxito.";
                return RedirectToAction(nameof(Index)); // <-- ¡La redirección!
            }

            // Si falla, volvemos a la vista con los datos
            return View(viewModel);
        }

        // --- ACCIÓN EDIT (MODIFICADA) ---
        [Route("Editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var servicio = await _context.Servicios
                .Include(s => s.VeterinariaDetalle)
                .AsNoTracking() // Importante para la edición
                .FirstOrDefaultAsync(s => s.Id == id && s.Tipo == TipoServicio.Veterinaria);
                
            if (servicio == null) return NotFound();
            
            // 1. Mapeamos de la BD al ViewModel
            var viewModel = new VeterinariaAdminViewModel
            {
                Id = servicio.Id,
                Nombre = servicio.Nombre,
                DescripcionCorta = servicio.DescripcionCorta,
                ImagenPrincipalUrl = servicio.ImagenPrincipalUrl,
                Direccion = servicio.VeterinariaDetalle?.Direccion,
                Telefono = servicio.VeterinariaDetalle?.Telefono,
                Horario = servicio.VeterinariaDetalle?.Horario,
                TelefonoSecundario = servicio.VeterinariaDetalle?.TelefonoSecundario,
                DescripcionLarga = servicio.VeterinariaDetalle?.DescripcionLarga
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VeterinariaAdminViewModel viewModel, IFormFile? nuevaImagen)
        {
            if (id != viewModel.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                var servicioToUpdate = await _context.Servicios
                    .Include(s => s.VeterinariaDetalle)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (servicioToUpdate == null) return NotFound();

                // 2. Mapeamos del ViewModel actualizado a la BD
                servicioToUpdate.Nombre = viewModel.Nombre;
                servicioToUpdate.DescripcionCorta = viewModel.DescripcionCorta;
                
                if (servicioToUpdate.VeterinariaDetalle == null)
                {
                    servicioToUpdate.VeterinariaDetalle = new VeterinariaDetalle();
                }
                
                servicioToUpdate.VeterinariaDetalle.Direccion = viewModel.Direccion;
                servicioToUpdate.VeterinariaDetalle.Telefono = viewModel.Telefono;
                servicioToUpdate.VeterinariaDetalle.Horario = viewModel.Horario;
                servicioToUpdate.VeterinariaDetalle.TelefonoSecundario = viewModel.TelefonoSecundario;
                servicioToUpdate.VeterinariaDetalle.DescripcionLarga = viewModel.DescripcionLarga;

                if (nuevaImagen != null)
                {
                    servicioToUpdate.ImagenPrincipalUrl = await SubirImagen(nuevaImagen);
                }

                try
                {
                    _context.Update(servicioToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Veterinaria actualizada con éxito.";
                    return RedirectToAction(nameof(Index)); // <-- ¡La redirección!
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. Fue modificado por otro usuario.");
                }
            }
            
            return View(viewModel);
        }

        // ... (Los métodos Delete y DeleteConfirmed se mantienen igual, usando el modelo Servicio) ...
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var servicio = await _context.Servicios
                .FirstOrDefaultAsync(m => m.Id == id && m.Tipo == TipoServicio.Veterinaria);
            if (servicio == null) return NotFound();
            return View(servicio);
        }

        // POST: Admin/Veterinarias/DeleteConfirmed/5
        [HttpPost]
        [Route("DeleteConfirmed/{id:int}")] // <-- CORRECCIÓN AQUÍ
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio != null)
            {
                // (Asegúrate de que las tablas relacionadas se borren en cascada si es necesario)
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Veterinaria eliminada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
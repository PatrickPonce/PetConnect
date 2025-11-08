using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using PetConnect.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")] // Solo para administradores
public class FaqsAdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public FaqsAdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: FaqsAdmin (Lista de preguntas)
    public async Task<IActionResult> Index()
    {
        var faqs = await _context.Faqs
            .OrderBy(f => f.Categoria)
            .ThenBy(f => f.Orden)
            .ToListAsync();
        return View(faqs);
    }

    // GET: FaqsAdmin/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FaqsAdmin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Pregunta,Respuesta,Categoria,Orden")] Faq faq)
    {
        if (ModelState.IsValid)
        {
            _context.Add(faq);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pregunta creada con éxito.";
            return RedirectToAction(nameof(Index));
        }
        return View(faq);
    }

    // GET: FaqsAdmin/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var faq = await _context.Faqs.FindAsync(id);
        if (faq == null) return NotFound();
        
        return View(faq);
    }

    // POST: FaqsAdmin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Pregunta,Respuesta,Categoria,Orden")] Faq faq)
    {
        if (id != faq.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(faq);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Pregunta actualizada con éxito.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Faqs.Any(e => e.Id == faq.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(faq);
    }

    // POST: FaqsAdmin/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var faq = await _context.Faqs.FindAsync(id);
        if (faq != null)
        {
            _context.Faqs.Remove(faq);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pregunta eliminada con éxito.";
        }
        return RedirectToAction(nameof(Index));
    }
}
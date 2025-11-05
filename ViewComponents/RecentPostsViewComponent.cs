using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.ViewComponents
{
    public class RecentPostsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RecentPostsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        // Aceptamos 'idActual' para excluir el post que el usuario ya está viendo
        public async Task<IViewComponentResult> InvokeAsync(int idActual)
        {
            var recentPosts = await _context.Noticias
                .Where(n => n.Id != idActual) // No mostrar el post actual
                .OrderByDescending(n => n.FechaPublicacion)
                .Take(4) // Trae los 4 más nuevos
                .ToListAsync();
            
            // Esto buscará el archivo Default.cshtml en la carpeta /Shared/Components/RecentPosts
            return View(recentPosts);
        }
    }
}
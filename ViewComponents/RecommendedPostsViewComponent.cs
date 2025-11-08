using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetConnect.Models; // Para Noticia
using PetConnect.Services; // Para MlNetRecommendationService
using System.Security.Claims; // Para ClaimsPrincipal
using System.Threading.Tasks;

namespace PetConnect.ViewComponents
{
    public class RecommendedPostsViewComponent : ViewComponent
    {
        private readonly MlNetRecommendationService _recommendationService;
        private readonly UserManager<IdentityUser> _userManager;

        public RecommendedPostsViewComponent(
            MlNetRecommendationService recommendationService, 
            UserManager<IdentityUser> userManager)
        {
            _recommendationService = recommendationService;
            _userManager = userManager;
        }

        // Este método se llamará desde la vista
        public async Task<IViewComponentResult> InvokeAsync(int currentNoticiaId)
        {
            // 1. Obtener el usuario actual
            var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);
            
            // 2. Si el usuario no está logueado, no podemos recomendar.
            if (user == null)
            {
                // Devolvemos una vista vacía
                return View(new List<Noticia>());
            }

            var userId = user.Id;
            
            // 3. ¡Llamar a tu servicio de IA!
            //    Pedimos 3 recomendaciones, excluyendo la noticia actual.
            var recommendations = _recommendationService.GetRecommendations(userId, currentNoticiaId, 3); 

            // 4. Enviamos la lista de noticias recomendadas a la vista
            return View(recommendations);
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers
{
    public class ServicioController : Controller
    {
        // ESTA ES LA ACCIÓN PARA LA PÁGINA PRINCIPAL DE SERVICIOS
        // Mostrará la barra de búsqueda y las categorías.
        public IActionResult Index(string busqueda)
        {
            // Puedes ver el término de búsqueda mientras depuras
            // System.Diagnostics.Debug.WriteLine($"Término de búsqueda: {busqueda}");

            if (!String.IsNullOrEmpty(busqueda))
            {
                
                
                // de la base de datos usando el término 'busqueda'
                // y pasar los resultados a la vista.
            }

            // Si no hay búsqueda, simplemente muestra todos los servicios o la vista por defecto.
            return View();
        }

        // Esta acción la usarás en el futuro para mostrar un servicio específico.
        public IActionResult Detalle(int id = 0)
        {
            // Aquí iría la lógica para buscar en la base de datos el servicio con el 'id'
            // y pasarlo a la vista.
            ViewData["Id"] = id;
            return View(); // Busca una vista llamada Detalle.cshtml
        }
    }   
}

using Microsoft.AspNetCore.Mvc;
using PetConnect.Models;

namespace PetConnect.Controllers
{
    public class ComentariosController : Controller
    {
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Comentario comentario)
        {
            TempData["MensajeComentario"] = "Comentario recibido (simulado).";
            return RedirectToAction("Index", "Noticias");
        }
    }
}

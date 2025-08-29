using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers
{
    public class NoticiasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detalle(int id = 0)
        {
            ViewData["Id"] = id;
            return View();
        }
    }
}
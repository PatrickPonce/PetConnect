using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace PetConnect.Controllers;

public class DirectorioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
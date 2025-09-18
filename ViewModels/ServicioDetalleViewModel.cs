using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetConnect.Models;

namespace PetConnect.ViewModels;

public class ServicioDetalleViewModel
{
    public Servicio Servicio { get; set; }
    public List<ResenaViewModel> Resenas { get; set; }
}
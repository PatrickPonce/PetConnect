using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.ViewModels;

public class ResenaViewModel
{
    public string Contenido { get; set; } = string.Empty;
    public short Puntuacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string? OcupacionUsuario { get; set; } // Campo extra del diseño
}
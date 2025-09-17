using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // IdentityUser ya tiene Id, UserName, Email, PasswordHash, etc.
    // Nosotros solo añadimos los campos personalizados que faltan de tu diseño.
    public string? NombreCompleto { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
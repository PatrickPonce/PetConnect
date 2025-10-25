// ViewModels/UsuarioViewModel.cs
using Microsoft.AspNetCore.Identity;

namespace PetConnect.ViewModels
{
    public class UsuarioViewModel
    {
        public required string Id { get; set; }
        public required string NombreUsuario { get; set; } // Guardamos el nombre aquí
        public string? Email { get; set; }
        public string? NumeroTelefono { get; set; }
        public string? ProfilePictureUrl { get; set; } // Para la foto de perfil
        // Puedes añadir más propiedades si las necesitas, como FechaRegistro
        public bool IsLockedOut { get; set; }
    }
}
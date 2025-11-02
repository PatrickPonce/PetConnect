namespace PetConnect.ViewModels
{
    public class UsuarioViewModel
    {
        public required string Id { get; set; }
        public required string NombreUsuario { get; set; }
        public string? Email { get; set; }
        public string? NumeroTelefono { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsLockedOut { get; set; }
        public string? RegistrationIp { get; set; }
    }
}
using PetConnect.Models;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.ViewModels
{
    // Este ViewModel combina los campos que necesitamos del
    // modelo Servicio y VeterinariaDetalle
    public class VeterinariaAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción Corta")]
        public string? DescripcionCorta { get; set; }

        [Display(Name = "Imagen Principal")]
        public string? ImagenPrincipalUrl { get; set; }

        // --- Campos de VeterinariaDetalle ---

        [Display(Name = "Dirección Completa")]
        public string? Direccion { get; set; }

        [Display(Name = "Teléfono Principal")]
        public string? Telefono { get; set; }

        [Display(Name = "Horario de Atención")]
        public string? Horario { get; set; }

        [Display(Name = "Teléfono de Emergencia (24h)")]
        public string? TelefonoSecundario { get; set; }

        [Display(Name = "Descripción Larga (Servicios)")]
        public string? DescripcionLarga { get; set; }
    }
}
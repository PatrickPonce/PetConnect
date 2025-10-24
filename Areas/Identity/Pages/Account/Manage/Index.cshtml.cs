// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims; // <--- AÑADIR
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // <--- AÑADIR
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetConnect.Claims; // <--- AÑADIR TU CLASE DE CLAIMS

namespace PetConnect.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager; // <--- AÑADIR
        private readonly IWebHostEnvironment _webHostEnvironment; // <--- AÑADIR

        // Constructor actualizado
        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, // <--- AÑADIR
            IWebHostEnvironment webHostEnvironment) // <--- AÑADIR
        {
            _userManager = userManager;
            _signInManager = signInManager; // <--- AÑADIR
            _webHostEnvironment = webHostEnvironment; // <--- AÑADIR
        }

        public string Username { get; set; }
        
        [Display(Name = "Foto de perfil")]
        public string ProfilePictureUrl { get; set; } // <--- AÑADIR (Para mostrar la foto actual)


        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Número de teléfono")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Nueva Foto de Perfil")]
            public IFormFile ProfilePictureFile { get; set; } // <--- AÑADIR (Para subir la foto)
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };

            // --- Cargar la Foto de Perfil desde los Claims ---
            var claims = await _userManager.GetClaimsAsync(user);
            ProfilePictureUrl = claims.FirstOrDefault(c => c.Type == PetConnectClaimTypes.ProfilePictureUrl)?.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar al usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar al usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // --- INICIO: LÓGICA DE SUBIDA DE FOTO ---
            if (Input.ProfilePictureFile != null)
            {
                // 1. Definir la carpeta de destino
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 2. Crear un nombre de archivo único (usando el ID del usuario)
                string fileExtension = Path.GetExtension(Input.ProfilePictureFile.FileName);
                string uniqueFileName = $"{user.Id}{fileExtension}"; // Ej: "guid-del-usuario.jpg"
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Guardar el archivo en el servidor
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePictureFile.CopyToAsync(fileStream);
                }

                // 4. Guardar la URL en los Claims del usuario
                string fileUrl = $"/images/avatars/{uniqueFileName}?v={DateTime.Now.Ticks}";

                var oldClaim = (await _userManager.GetClaimsAsync(user))
                                .FirstOrDefault(c => c.Type == PetConnectClaimTypes.ProfilePictureUrl);

                if (oldClaim != null)
                {
                    // Si ya tenía una foto, la reemplazamos
                    await _userManager.RemoveClaimAsync(user, oldClaim);
                }
                
                // Añadimos el nuevo claim
                await _userManager.AddClaimAsync(user, new Claim(PetConnectClaimTypes.ProfilePictureUrl, fileUrl));

                // 5. Refrescar la cookie de sesión para que la navbar vea el cambio
                await _signInManager.RefreshSignInAsync(user);
            }
            // --- FIN: LÓGICA DE SUBIDA DE FOTO ---

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Error inesperado al intentar establecer el número de teléfono.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Tu perfil ha sido actualizado";
            return RedirectToPage();
        }
    }
}
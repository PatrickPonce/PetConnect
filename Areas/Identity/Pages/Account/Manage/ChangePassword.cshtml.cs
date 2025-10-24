// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace PetConnect.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
        /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
        /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
        /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
            /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
            /// </summary>
            [Required(ErrorMessage = "El campo {0} es obligatorio.")] // TRADUCIDO
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña actual")] // TRADUCIDO: "Current password"
            public string OldPassword { get; set; }

            /// <summary>
            /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
            /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
            /// </summary>
            [Required(ErrorMessage = "El campo {0} es obligatorio.")] // TRADUCIDO
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)] // TRADUCIDO
            [DataType(DataType.Password)]
            [Display(Name = "Nueva contraseña")] // TRADUCIDO: "New password"
            public string NewPassword { get; set; }

            /// <summary>
            /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
            /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nueva contraseña")] // TRADUCIDO: "Confirm new password"
            [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la contraseña de confirmación no coinciden.")] // TRADUCIDO
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // TRADUCIDO
                return NotFound($"No se puede cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // TRADUCIDO
                return NotFound($"No se puede cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            // TRADUCIDO: "User changed their password successfully."
            _logger.LogInformation("El usuario cambió su contraseña exitosamente."); 
            
            // TRADUCIDO: "Your password has been changed."
            StatusMessage = "Tu contraseña ha sido cambiada."; 

            return RedirectToPage();
        }
    }
}
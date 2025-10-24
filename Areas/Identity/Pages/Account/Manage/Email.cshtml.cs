// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace PetConnect.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
        /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 	Esta API es compatible con la infraestructura de interfaz de usuario predeterminada de ASP.NET Core Identity y no está diseñada para ser utilizada
        /// 	directamente desde su código. Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

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
        [BindProperty]
        public InputModel Input { get; set; }

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
            [Required]
            [EmailAddress]
            [Display(Name = "Nuevo correo electrónico")] // TRADUCIDO
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // TRADUCIDO
                return NotFound($"No se puede cargar el usuario con ID '{_userManager.GetUserId(User)}'."); 
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // TRADUCIDO
                return NotFound($"No se puede cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Confirma tu correo electrónico", // TRADUCIDO: "Confirm your email"
                    $"Por favor, confirma tu cuenta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>."); // TRADUCIDO

                // TRADUCIDO
                StatusMessage = "Enlace de confirmación para cambiar el correo electrónico enviado. Por favor, revisa tu correo."; 
                return RedirectToPage();
            }

            // TRADUCIDO
            StatusMessage = "Tu correo electrónico no ha cambiado.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // TRADUCIDO
                return NotFound($"No se puede cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirma tu correo electrónico", // TRADUCIDO: "Confirm your email"
                $"Por favor, confirma tu cuenta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>."); // TRADUCIDO

            // TRADUCIDO
            StatusMessage = "Correo electrónico de verificación enviado. Por favor, revisa tu correo.";
            return RedirectToPage();
        }
    }
}
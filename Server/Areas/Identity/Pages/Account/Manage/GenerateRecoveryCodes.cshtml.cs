using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoBlazorAuthentication.Server.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DemoBlazorAuthentication.Server.Areas.Identity.Pages.Account.Manage
{
    public class GenerateRecoveryCodesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GenerateRecoveryCodesModel> _logger;

        public GenerateRecoveryCodesModel(
            UserManager<ApplicationUser> userManager,
            ILogger<GenerateRecoveryCodesModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Non � stato possibile trovare il profilo utenti con ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                throw new InvalidOperationException($"Non � stato possibile generare codici di recupero per il profilo utente con ID '{userId}' perch� l'autenticazione a due fattori non � abilitata.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Non � stato possibile trovare il profilo utente con ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException($"Non � stato possibile generare i codici di recupero per il profilo utente con ID '{userId}' perch� l'autenticazione a due fattori non � abilitata.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
            StatusMessage = "Hai generato dei nuovi codici di recupero.";
            return RedirectToPage("./ShowRecoveryCodes");
        }
    }
}
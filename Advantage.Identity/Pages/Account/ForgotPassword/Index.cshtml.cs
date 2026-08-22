using System.ComponentModel.DataAnnotations;
using Advantage.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Advantage.Identity.Pages.Account.ForgotPassword;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public Index(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool EmailSent { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        // Always show the same confirmation regardless of whether the user exists,
        // so this endpoint can't be used to enumerate registered email addresses.
        if (user != null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
            var resetUrl = Url.Page(
                "/Account/ResetPassword/Index",
                pageHandler: null,
                values: new { userId = user.Id, token = encodedToken },
                protocol: Request.Scheme);

            // Advantage.Notifications doesn't exist yet (see plan Phase 7) — for the POC
            // the reset link is logged instead of emailed.
            Serilog.Log.Information("Password reset requested for {Email}. Reset link: {ResetUrl}", Input.Email, resetUrl);
        }

        EmailSent = true;
        return Page();
    }
}

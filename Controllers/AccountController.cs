using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolBillingERP.Models;
using System.Text;

namespace SchoolBillingERP.Controllers
{
    [Authorize]

    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Support login with username or email
                var user = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Username);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.RequiresTwoFactor)
                {
                    return Redirect($"/Identity/Account/LoginWith2fa?rememberMe={model.RememberMe}");
                }
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirmation do not match.");
                return View();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Message"] = "Password changed successfully.";
                TempData["MessageType"] = "success";
                return RedirectToAction("ChangePassword", "Account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");
            var model = new ChangeEmailViewModel
            {
                OldEmail = user.Email
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmailAsync(string newEmail)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            var model = new ChangeEmailViewModel
            {
                OldEmail = user.Email
            };
            if (user.Email == newEmail)
            {
                TempData["Message"] = "Old Email and New email are same!";
                TempData["MessageType"] = "error";
                return RedirectToAction("ChangeEmail");


            }
            var result = await _userManager.SetEmailAsync(user, newEmail);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            user.EmailConfirmed = true;
            var confirmResult = await _userManager.UpdateAsync(user);
            if (!confirmResult.Succeeded)
            {
                TempData["Message"] = "Email was updated but could not be confirmed.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ChangeEmail");
            }

            TempData["Message"] = "Email Changed successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("ChangeEmail");
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Message"] = "You have been logged out.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login");
        }



        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(authenticatorKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

            }
            var sharedKey = FormatKey(authenticatorKey);
            var qrCode = GenerateQrCodeUri(user.Email, authenticatorKey);
            var model = new EnableAuthenticatorViewModel
            {
                SharedKey = sharedKey,
                AuthenticatorUri = qrCode
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                 user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);

            if (!isValid)
            {
                ModelState.AddModelError("Code", "Invalid verification code.");

                // Rebuild the QR code and shared key for the view
                var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(authenticatorKey))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
                }

                var sharedKey = FormatKey(authenticatorKey);
                var qrCode = GenerateQrCodeUri(user.Email, authenticatorKey);

                // Repopulate model
                model.SharedKey = sharedKey;
                model.AuthenticatorUri = qrCode;

                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["Message"] = "Two-factor authentication has been enabled.";
            TempData["MessageType"] = "success";


            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Disable2FAAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Check if 2FA is enabled
            var is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!is2faEnabled)
            {
                TempData["Message"] = "Two-factor authentication is already disabled.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            // Disable 2FA
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            TempData["Message"] = "Two-factor authentication has been disabled.";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Home");
        }
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
       "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
       Uri.EscapeDataString("SchoolBillingERP"),  // issuer
       Uri.EscapeDataString(email),                     // account name
       unformattedKey);
        }


        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            var currentPosition = 0;

            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

    }
}

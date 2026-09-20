using CustomerCampaign.Portal.Models;
using CustomerCampaign.Portal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomerCampaign.Portal.Controllers
{
    [AllowAnonymous]
    public class AccountController(CampaignApiClient api) : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var token = await api.LoginAsync(model.Username, model.Password, ct);

                var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, token.Subject),
                new(ClaimTypes.Name, model.Username),
                new(ClaimTypes.Role, token.Role),
                new("access_token", token.AccessToken)
            };

                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn)
                    });

                return LocalRedirect(model.ReturnUrl ?? "/");
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.StatusCode == 401 ? "Invalid username or password." : ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}

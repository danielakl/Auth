namespace Auth.Controllers.Web
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Auth.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            this.ViewData["ReturnUrl"] = model.ReturnUrl;

            if (this.ModelState.IsValid)
            {
                // TODO: Check credentials before adding claims
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, model.Username),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await this.HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

                // Check local to prevent open redirect attacks
                // https://docs.microsoft.com/en-us/aspnet/core/security/preventing-open-redirects?view=aspnetcore-5.0
                if (this.Url.IsLocalUrl(model.ReturnUrl))
                {
                    return this.Redirect(model.ReturnUrl);
                }

                return this.RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return this.View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync();

            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}

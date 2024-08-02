using LotteryResult.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LotteryResult.Controllers
{
    public class AuthController : Controller
    {
        private IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            var user = HttpContext.User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel request)
        {
            //if (string.IsNullOrEmpty(request.UserName))
            //{
            //    ModelState.AddModelError("UserName", "Correo Requerido");
            //}

            //if (string.IsNullOrEmpty(request.Password))
            //{
            //    ModelState.AddModelError("Password", "Contraseña Requerido");
            //}

            if (!ModelState.IsValid)
            {
                return View(request);
            }
            
            var pass = _configuration.GetValue<string>("AdminPass");
            if (request.UserName != "admin" || request.Password != pass)
            {
                ModelState.AddModelError("_", "Correo o contraseña invalidos");
                return View(request);
            }

            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.Name, "admin")
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                //IsPersistent = false,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("login");
        }
    }
}

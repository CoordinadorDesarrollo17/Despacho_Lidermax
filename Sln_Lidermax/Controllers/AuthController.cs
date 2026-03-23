using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Dtos;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Services;
using System.Security.Claims;

namespace Sln_Lidermax.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var isValid = authService.ValidateUser(model.Username, model.Password);

            if (!isValid)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View(model);
            }

  
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username)    
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Denied()
        {
            return View();
        }
    }
}

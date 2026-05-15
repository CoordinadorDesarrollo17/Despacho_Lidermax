using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using Sln_Lidermax.Services;
using System.Security.Claims;

namespace Sln_Lidermax.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthService authService;
        private readonly UserManager<UsuarioModel> userManager;
        private readonly SignInManager<UsuarioModel> signInManager;

        public AuthController(IAuthService authService, UserManager<UsuarioModel> userManager, SignInManager<UsuarioModel> signInManager)
        {
            this.authService = authService;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }
            var usuarioDb = await authService.BuscarUsuario(modelo.Username);
            if (usuarioDb == null)
            {
                ModelState.AddModelError("", "Nombre de usuario incorrecto.");
                return View(modelo);
            }
            if (usuarioDb.Activo == 0)
            {
                ModelState.AddModelError("", "El usuario se encuentra inactivo.");
                return View(modelo);
            }
            var resultado = await signInManager.PasswordSignInAsync(modelo.Username, modelo.Password, true, lockoutOnFailure: false);

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError("", "Nombre de usuario o password incorrecto.");
                return View(modelo);
            }
            var user = await userManager.FindByNameAsync(modelo.Username);
            var claims = new List<Claim>
            {
                new Claim("NombreCompleto", usuarioDb?.Nombres + " " + usuarioDb?.Apellidos ?? modelo.Username),
                new Claim("IdRol", usuarioDb.IdRol.ToString()),
                new Claim(ClaimTypes.Role, usuarioDb.Prefijo)
            };

            await signInManager.SignOutAsync(); //limpiamos claims
            await signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims); //agregamos claims

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using StarGuinchos.ViewModels;
using StarGuinchos.Filters;

namespace StarGuinchos.Controllers
{
   
    public class AdminAuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("admin/login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetString("AdminLogado") == "true")
            {
                return RedirectToAction("Index", "Admin");
            }

            return View(new AdminLoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost("admin/login")]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioConfigurado = _configuration["AdminAccess:Username"];
            var senhaConfigurada = _configuration["AdminAccess:Password"];

            if (string.IsNullOrWhiteSpace(usuarioConfigurado) ||
                string.IsNullOrWhiteSpace(senhaConfigurada))
            {
                ModelState.AddModelError(string.Empty, "Login administrativo não configurado.");
                return View(model);
            }

            var usuarioValido = model.Username == usuarioConfigurado;
            var senhaValida = model.Password == senhaConfigurada;

            if (!usuarioValido || !senhaValida)
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                return View(model);
            }

            HttpContext.Session.SetString("AdminLogado", "true");
            HttpContext.Session.SetString("AdminUsuario", model.Username);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet("admin/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
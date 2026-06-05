using Microsoft.AspNetCore.Mvc;
using PA_WEB.Models;

namespace PA_WEB.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new InicioSesionModel());
        }

        [HttpPost]
        public IActionResult IniciarSesion(InicioSesionModel model)
        {
            return RedirectToAction("Inicio", "Home");
        }

        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            return View(new RegistrarUsuarioModel());
        }

        [HttpPost]
        public IActionResult RegistrarUsuario(RegistrarUsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["MensajeRegistro"] = "Usuario registrado correctamente. Ahora puede iniciar sesión.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Inicio()
        {
            return View();
        }
    }
}
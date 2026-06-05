using Microsoft.AspNetCore.Mvc;
using PA_WEB.Models;

namespace PA_WEB.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IniciarSesion(InicioSesionModel model)
        {
            // implementar logica de inicio
            return RedirectToAction("Inicio", "Home");
        }

        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            return View();
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

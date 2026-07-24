using Microsoft.AspNetCore.Mvc;
using PA_WEB.Models;

namespace PA_WEB.Controllers
{
    public class HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new InicioSesionModel());
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(InicioSesionModel model)
        {
            using var client = httpClientFactory.CreateClient();
            var urlApi = configuration["Valores:UrlApi"] + "usuarios/IniciarSesion";

            var request = new
            {
                model.CorreoElectronico,
                model.Contrasenna
            };

            var response = await client.PostAsJsonAsync(urlApi, request);
            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<InicioSesionResponseModel>>();

            if (response.IsSuccessStatusCode && resultado?.Data is not null)
            {
                var usuario = resultado.Data.Usuario;

                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
                HttpContext.Session.SetString("CorreoUsuario", usuario.CorreoElectronico);
                HttpContext.Session.SetString("TelefonoUsuario", usuario.Telefono);
                HttpContext.Session.SetString("IdentificacionUsuario", usuario.Identificacion);
                HttpContext.Session.SetString("FechaNacimientoUsuario", usuario.FechaNacimiento.ToString("yyyy-MM-dd"));
                HttpContext.Session.SetString("Token", resultado.Data.Token);

                return RedirectToAction("Inicio", "Home");
            }

            ViewBag.Mensaje = resultado?.Message ?? "No se pudo iniciar sesión. Verifique sus datos.";
            return View("Index", model);
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
        public async Task<IActionResult> Inicio()
        {
            using var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(configuration["Valores:UrlApi"] + "especialidades");

            var especialidades = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<EspecialidadModel>>()
                : [];

            return View(especialidades ?? []);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using PA_WEB.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PA_WEB.Controllers
{
    public class HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
    {
        private string UrlApi => configuration["Valores:UrlApi"]!;

        private HttpClient CrearCliente()
        {
            return httpClientFactory.CreateClient();
        }

        private HttpClient CrearClienteAutenticado()
        {
            var client = httpClientFactory.CreateClient();

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        [HttpGet]
        public IActionResult Index(string? mensaje)
        {
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                TempData["Mensaje"] = mensaje;
            }

            return View(new InicioSesionModel());
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(InicioSesionModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CorreoElectronico) || string.IsNullOrWhiteSpace(model.Contrasenna))
            {
                ViewBag.Mensaje = "Debe ingresar el correo electrónico y la contraseña.";
                return View("Index", model);
            }

            try
            {
                using var client = CrearCliente();

                var request = new
                {
                    model.CorreoElectronico,
                    model.Contrasenna
                };

                var response = await client.PostAsJsonAsync(UrlApi + "usuarios/IniciarSesion", request);
                var resultado = await response.Content.ReadFromJsonAsync<ResultModel<InicioSesionResponseModel>>();

                if (response.IsSuccessStatusCode && resultado?.Success == true && resultado.Data is not null)
                {
                    var usuario = resultado.Data.Usuario;

                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
                    HttpContext.Session.SetString("CorreoUsuario", usuario.CorreoElectronico);
                    HttpContext.Session.SetString("TelefonoUsuario", usuario.Telefono);
                    HttpContext.Session.SetString("IdentificacionUsuario", usuario.Identificacion);
                    HttpContext.Session.SetString("FechaNacimientoUsuario", usuario.FechaNacimiento.ToString("yyyy-MM-dd"));
                    HttpContext.Session.SetString("Token", resultado.Data.Token);

                    if (resultado.Data.TemporaryPassword)
                    {
                        TempData["Mensaje"] = "Ingresó con una contraseña temporal. Debe actualizar su contraseña antes de continuar.";
                        return RedirectToAction("ActualizarContrasena", "Home");
                    }

                    return RedirectToAction("Inicio", "Home");
                }

                ViewBag.Mensaje = resultado?.Message ?? "No se pudo iniciar sesión. Verifique sus datos.";
                return View("Index", model);
            }
            catch
            {
                ViewBag.Mensaje = "No se pudo conectar con el API. Verifique que PA_API esté ejecutándose.";
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            return View(new RegistrarUsuarioModel());
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario(RegistrarUsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var client = CrearCliente();

                var request = new
                {
                    model.Identificacion,
                    NombreCompleto = model.Nombre,
                    model.CorreoElectronico,
                    model.Telefono,
                    FechaNacimiento = model.FechaNacimiento!.Value,
                    model.Contrasenna,
                    model.ConfirmarContrasenna
                };

                var response = await client.PostAsJsonAsync(UrlApi + "usuarios/Registrar", request);
                var resultado = await response.Content.ReadFromJsonAsync<ResultModel<UsuarioModel>>();

                if (response.IsSuccessStatusCode && resultado?.Success == true && resultado.Data is not null)
                {
                    TempData["MensajeRegistro"] = "Usuario registrado correctamente. Ahora puede iniciar sesión.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = resultado?.Message ?? "No se pudo registrar el usuario.";
                return View(model);
            }
            catch
            {
                ViewBag.Mensaje = "No se pudo conectar con el API. Verifique que PA_API esté ejecutándose.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View(new RecuperarAccesoModel());
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarAcceso(RecuperarAccesoModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var client = CrearCliente();

                var request = new
                {
                    model.CorreoElectronico
                };

                var response = await client.PostAsJsonAsync(UrlApi + "usuarios/RecuperarAcceso", request);
                var resultado = await response.Content.ReadFromJsonAsync<ResultModel>();

                if (response.IsSuccessStatusCode && resultado?.Success == true)
                {
                    TempData["MensajeRecuperar"] = resultado.Message ?? "Si el correo existe, recibirá un correo con instrucciones.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = resultado?.Message ?? "No se pudo recuperar el acceso.";
                return View(model);
            }
            catch
            {
                ViewBag.Mensaje = "No se pudo conectar con el API. Verifique que PA_API esté ejecutándose.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ActualizarContrasena()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var token = HttpContext.Session.GetString("Token");

            if (usuarioId is null || string.IsNullOrWhiteSpace(token))
            {
                TempData["Mensaje"] = "Debe iniciar sesión para actualizar su contraseña.";
                return RedirectToAction("Index", "Home");
            }

            return View(new ActualizarContrasenaModel());
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarContrasena(ActualizarContrasenaModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var token = HttpContext.Session.GetString("Token");

            if (usuarioId is null || string.IsNullOrWhiteSpace(token))
            {
                TempData["Mensaje"] = "Debe iniciar sesión para actualizar su contraseña.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var client = CrearClienteAutenticado();

                var request = new
                {
                    Id = usuarioId.Value,
                    model.ContrasenaNueva,
                    model.ConfirmarContrasenaNueva
                };

                var response = await client.PutAsJsonAsync(UrlApi + "usuarios/ActualizarContrasena", request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    TempData["Mensaje"] = "La sesión expiró o el token no es válido. Inicie sesión nuevamente.";
                    return RedirectToAction("Index", "Home");
                }

                var resultado = await response.Content.ReadFromJsonAsync<ResultModel>();

                if (response.IsSuccessStatusCode && resultado?.Success == true)
                {
                    TempData["Mensaje"] = resultado.Message ?? "Contraseña actualizada correctamente.";
                    return RedirectToAction("Inicio", "Home");
                }

                ViewBag.Mensaje = resultado?.Message ?? "No se pudo actualizar la contraseña.";
                return View(model);
            }
            catch
            {
                ViewBag.Mensaje = "No se pudo conectar con el API. Verifique que PA_API esté ejecutándose.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Inicio()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var token = HttpContext.Session.GetString("Token");

            if (usuarioId is null || string.IsNullOrWhiteSpace(token))
            {
                TempData["Mensaje"] = "Debe iniciar sesión para acceder al sistema.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using var client = CrearClienteAutenticado();

                var response = await client.GetAsync(UrlApi + "especialidades");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();
                    TempData["Mensaje"] = "La sesión expiró o el token no es válido. Inicie sesión nuevamente.";
                    return RedirectToAction("Index", "Home");
                }

                var especialidades = response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<List<EspecialidadModel>>()
                    : new List<EspecialidadModel>();

                return View(especialidades ?? new List<EspecialidadModel>());
            }
            catch
            {
                TempData["Mensaje"] = "No se pudieron cargar las especialidades. Verifique que PA_API esté ejecutándose.";
                return View(new List<EspecialidadModel>());
            }
        }

        [HttpGet]
        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Index", "Home");
        }
    }
}
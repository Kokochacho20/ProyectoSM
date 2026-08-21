using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using PA_WEB.Services;

namespace PA_WEB.Controllers
{
    public class HomeController(
        IUsuarioService usuarioService,
        IProfesionalService profesionalService) : Controller
    {
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
            if (string.IsNullOrWhiteSpace(model.CorreoElectronico) ||
                string.IsNullOrWhiteSpace(model.Contrasenna))
            {
                ViewBag.Mensaje = "Debe ingresar el correo electrónico y la contraseña.";
                return View("Index", model);
            }

            try
            {
                var response = await usuarioService.IniciarSesionAsync(
                    model.CorreoElectronico,
                    model.Contrasenna);

                if (response != null && response.Success && response.Data is not null)
                {
                    var usuario = response.Data.Usuario;

                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
                    HttpContext.Session.SetString("CorreoUsuario", usuario.CorreoElectronico);
                    HttpContext.Session.SetString("TelefonoUsuario", usuario.Telefono);
                    HttpContext.Session.SetString("IdentificacionUsuario", usuario.Identificacion);
                    HttpContext.Session.SetString("FechaNacimientoUsuario", usuario.FechaNacimiento.ToString("yyyy-MM-dd"));
                    HttpContext.Session.SetString("Token", response.Data.Token);

                    HttpContext.Session.SetInt32("RolId", usuario.RolId);
                    HttpContext.Session.SetString("RolNombre", usuario.RolNombre);

                    if (usuario.ProfesionalMedicoId.HasValue)
                    {
                        HttpContext.Session.SetInt32(
                            "ProfesionalMedicoId",
                            usuario.ProfesionalMedicoId.Value);
                    }
                    else
                    {
                        HttpContext.Session.Remove("ProfesionalMedicoId");
                    }

                    if (!string.IsNullOrWhiteSpace(usuario.ProfesionalNombre))
                    {
                        HttpContext.Session.SetString(
                            "ProfesionalNombre",
                            usuario.ProfesionalNombre);
                    }
                    else
                    {
                        HttpContext.Session.Remove("ProfesionalNombre");
                    }

                    if (response.Data.TemporaryPassword)
                    {
                        TempData["Mensaje"] = "Ingresó con una contraseña temporal. Debe actualizar su contraseña antes de continuar.";
                        return RedirectToAction("ActualizarContrasena", "Home");
                    }

                    return RedireccionarSegunRol();
                }

                ViewBag.Mensaje = response?.Message ?? "No se pudo iniciar sesión. Verifique sus datos.";
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
                var response = await usuarioService.RegistrarUsuarioAsync(model);

                if (response != null && response.Success)
                {
                    TempData["MensajeRegistro"] = "Usuario registrado correctamente. Ahora puede iniciar sesión.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = response?.Message ?? "No se pudo registrar el usuario.";
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
                var response = await usuarioService.RecuperarAccesoAsync(
                    model.CorreoElectronico);

                if (response != null && response.Success)
                {
                    TempData["MensajeRecuperar"] = response.Message ?? "Si el correo existe, recibirá un correo con instrucciones.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = response?.Message ?? "No se pudo recuperar el acceso.";
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

        [RequiereSesion]
        [HttpPost]
        public async Task<IActionResult> ActualizarContrasena(ActualizarContrasenaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await usuarioService.ActualizarContrasennaAsync(
                    model.ContrasenaNueva,
                    model.ConfirmarContrasenaNueva);

                if (response != null && response.Success)
                {
                    TempData["Mensaje"] = response.Message ?? "Contraseña actualizada correctamente.";
                    return RedireccionarSegunRol();
                }

                ViewBag.Mensaje = response?.Message ?? "No se pudo actualizar la contraseña.";
                return View(model);
            }
            catch
            {
                ViewBag.Mensaje = "No se pudo conectar con el API. Verifique que PA_API esté ejecutándose.";
                return View(model);
            }
        }

        [RequiereSesion]
        [HttpGet]
        public async Task<IActionResult> Inicio()
        {
            try
            {
                var especialidades = await profesionalService.ObtenerEspecialidadesAsync();
                return View(especialidades);
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

        private IActionResult RedireccionarSegunRol()
        {
            var rolId = HttpContext.Session.GetInt32("RolId");

            return rolId switch
            {
                1 => RedirectToAction("Index", "Admin"),
                2 => RedirectToAction("Index", "Medico"),
                _ => RedirectToAction("Inicio", "Home")
            };
        }
    }
}
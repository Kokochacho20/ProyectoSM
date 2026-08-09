using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class CitasController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
    {
        private string UrlApi => configuration["Valores:UrlApi"]!;

        private HttpClient CrearCliente()
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

        private IActionResult RedirigirSesionExpirada()
        {
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "La sesión expiró o el token no es válido. Inicie sesión nuevamente.";
            return RedirectToAction("Index", "Home");
        }

        private async Task<List<EspecialidadModel>> ConsultarEspecialidadesAsync()
        {
            using var client = CrearCliente();

            var response = await client.GetAsync($"{UrlApi}especialidades");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new List<EspecialidadModel>();
            }

            if (!response.IsSuccessStatusCode)
            {
                return new List<EspecialidadModel>();
            }

            var especialidades = await response.Content.ReadFromJsonAsync<List<EspecialidadModel>>();

            return especialidades ?? new List<EspecialidadModel>();
        }

        [HttpGet]
        public async Task<IActionResult> Profesionales(string? texto, int? especialidadId)
        {
            using var client = CrearCliente();

            var especialidades = await ConsultarEspecialidadesAsync();

            ViewBag.Texto = texto;
            ViewBag.EspecialidadId = especialidadId;
            ViewBag.Especialidades = especialidades;

            var parametros = new List<string>();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                parametros.Add($"texto={Uri.EscapeDataString(texto)}");
            }

            if (especialidadId.HasValue && especialidadId.Value > 0)
            {
                parametros.Add($"especialidadId={especialidadId.Value}");
            }

            var query = parametros.Count > 0
                ? "?" + string.Join("&", parametros)
                : string.Empty;

            var response = await client.GetAsync($"{UrlApi}profesionales{query}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirSesionExpirada();
            }

            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<List<ProfesionalModel>>>();

            if (!response.IsSuccessStatusCode || resultado?.Data is null)
            {
                ViewBag.Mensaje = resultado?.Message ?? "No se pudieron cargar los profesionales.";
                return View(new List<ProfesionalModel>());
            }

            return View(resultado.Data);
        }

        [HttpGet]
        public IActionResult Crear(int profesionalId, string profesionalNombre)
        {
            var model = new CrearCitaModel
            {
                ProfesionalMedicoId = profesionalId,
                ProfesionalMedicoNombre = profesionalNombre,
                Fecha = DateTime.Today,
                Hora = new TimeSpan(8, 0, 0),
                NombrePaciente = HttpContext.Session.GetString("NombreUsuario") ?? string.Empty,
                IdentificacionPaciente = HttpContext.Session.GetString("IdentificacionUsuario") ?? string.Empty,
                CorreoPaciente = HttpContext.Session.GetString("CorreoUsuario") ?? string.Empty,
                TelefonoPaciente = HttpContext.Session.GetString("TelefonoUsuario") ?? string.Empty,
                FechaNacimientoPaciente = DateTime.TryParse(
                    HttpContext.Session.GetString("FechaNacimientoUsuario"),
                    out var fechaNacimiento) ? fechaNacimiento : DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearCitaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var fechaHoraInicio = model.Fecha.Date + model.Hora;

            if (fechaHoraInicio.Year < 1753)
            {
                ViewBag.Mensaje = "Debe seleccionar una fecha y hora válida para la cita.";
                return View(model);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            var request = new
            {
                UsuarioId = usuarioId,
                model.ProfesionalMedicoId,
                FechaHoraInicio = fechaHoraInicio,
                model.EsParaOtraPersona,
                model.NombrePaciente,
                model.IdentificacionPaciente,
                model.FechaNacimientoPaciente,
                model.CorreoPaciente,
                model.TelefonoPaciente,
                model.Motivo
            };

            using var client = CrearCliente();

            var response = await client.PostAsJsonAsync($"{UrlApi}citas/usuario", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirSesionExpirada();
            }

            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Mensaje = resultado?.Message ?? "No se pudo registrar la cita.";
                return View(model);
            }

            TempData["Mensaje"] = "Cita registrada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            using var client = CrearCliente();

            var response = await client.GetAsync($"{UrlApi}citas?usuarioId={usuarioId}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirSesionExpirada();
            }

            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<List<CitaModel>>>();

            return View(resultado?.Data ?? new List<CitaModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Modificar(int citaId)
        {
            using var client = CrearCliente();

            var response = await client.GetAsync($"{UrlApi}citas/{citaId}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirSesionExpirada();
            }

            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();

            if (!response.IsSuccessStatusCode || resultado?.Data is null)
            {
                TempData["Mensaje"] = "Cita no encontrada.";
                return RedirectToAction("Index");
            }

            var cita = resultado.Data;

            ViewBag.ProfesionalMedico = cita.ProfesionalMedico;

            return View(new ModificarCitaModel
            {
                CitaId = cita.Id,
                Fecha = cita.FechaHoraInicio.Date,
                Hora = cita.FechaHoraInicio.TimeOfDay
            });
        }

        [HttpPost]
        public async Task<IActionResult> Modificar(ModificarCitaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            var request = new
            {
                UsuarioId = usuarioId,
                FechaHoraInicio = model.Fecha.Date + model.Hora
            };

            using var client = CrearCliente();

            var response = await client.PutAsJsonAsync($"{UrlApi}citas/{model.CitaId}", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirSesionExpirada();
            }

            if (!response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();
                ViewBag.Mensaje = resultado?.Message ?? "No se pudo modificar la cita.";
                return View(model);
            }

            TempData["Mensaje"] = "Cita modificada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int citaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            using var client = CrearCliente();

            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{UrlApi}citas/{citaId}/cancelar")
            {
                Content = JsonContent.Create(new { UsuarioId = usuarioId })
            };

            var response = await client.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirigirSesionExpirada();
            }

            var resultado = await response.Content.ReadFromJsonAsync<ResultModel>();

            TempData["Mensaje"] = resultado?.Message
                ?? (response.IsSuccessStatusCode ? "Cita cancelada." : "No se pudo cancelar la cita.");

            return RedirectToAction("Index");
        }
    }
}
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

        [HttpGet]
        public async Task<IActionResult> Profesionales(int? especialidadId, string? texto)
        {
            using var client = CrearCliente();

// Construir query string solo con parámetros presentes
var url = $"{UrlApi}profesionales";
var query = new List<string>();
if (especialidadId.HasValue && especialidadId.Value != 0)
    query.Add($"especialidadId={especialidadId.Value}");
if (!string.IsNullOrWhiteSpace(texto))
    query.Add($"texto={Uri.EscapeDataString(texto)}");
if (query.Any())
    url += "?" + string.Join("&", query);

var response = await client.GetAsync(url);

if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    return RedirigirSesionExpirada();
}

            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<List<ProfesionalModel>>>();

            if (!response.IsSuccessStatusCode || resultado?.Data is null)
            {
                TempData["Mensaje"] = resultado?.Message ?? "No se pudieron cargar los profesionales.";
                return RedirectToAction("Inicio", "Home");
            }

            // Obtener la lista de especialidades para el combo (Todas las especialidades)
            var respEsps = await client.GetAsync($"{UrlApi}especialidades");
            List<EspecialidadModel>? especialidades = null;
            if (respEsps.IsSuccessStatusCode)
            {
                // Many APIs return a plain array for this endpoint. Try to read as List<T> first
                // and fall back to the wrapped ResultModel<T> shape if necessary.
                try
                {
                    especialidades = await respEsps.Content.ReadFromJsonAsync<List<EspecialidadModel>>();
                }
                catch (System.Text.Json.JsonException)
                {
                    var resultadoEsps = await respEsps.Content.ReadFromJsonAsync<ResultModel<List<EspecialidadModel>>>();
                    especialidades = resultadoEsps?.Data;
                }
            }

            ViewBag.Especialidades = especialidades ?? new List<EspecialidadModel>();

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

            // Validate and compose FechaHoraInicio explicitly to avoid culture/format issues
            // model.Fecha is DateTime with date component, model.Hora is TimeSpan
            var fecha = model.Fecha.Date;
            var hora = model.Hora;
            var fechaHoraInicio = fecha + hora; // DateTime + TimeSpan

            var request = new
            {
usuarioId = usuarioId,
profesionalMedicoId = model.ProfesionalMedicoId,
fechaHoraInicio = fechaHoraInicio,
esParaOtraPersona = model.EsParaOtraPersona,
nombrePaciente = model.NombrePaciente,
identificacionPaciente = model.IdentificacionPaciente,
fechaNacimientoPaciente = model.FechaNacimientoPaciente,
correoPaciente = model.CorreoPaciente,
telefonoPaciente = model.TelefonoPaciente,
motivo = model.Motivo

            };

            using var client = CrearCliente();

            var response = await client.PostAsJsonAsync($"{UrlApi}citas/usuario", request);

if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    return RedirigirSesionExpirada();
}

var responseBody = await response.Content.ReadAsStringAsync();

ResultModel<CitaModel>? resultado = null;
try
{
    resultado = System.Text.Json.JsonSerializer.Deserialize<ResultModel<CitaModel>>(responseBody, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}
catch (System.Text.Json.JsonException)
{
    // ignore deserialization failure; we'll use raw responseBody for messages
}


            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Mensaje = resultado?.Message ?? responseBody ?? "No se pudo registrar la cita.";
                return View(model);
            }

            TempData["Mensaje"] = resultado?.Message ?? "Cita registrada correctamente.";
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
                var responseBody = await response.Content.ReadAsStringAsync();
                var resultado = await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();
                ViewBag.Mensaje = resultado?.Message ?? responseBody ?? "No se pudo modificar la cita.";
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

            if (resultado is null || string.IsNullOrWhiteSpace(resultado.Message))
            {
                var body = await response.Content.ReadAsStringAsync();
                // no logging, keep silent in production
            }

            TempData["Mensaje"] = resultado?.Message
                ?? (response.IsSuccessStatusCode ? "Cita cancelada." : "No se pudo cancelar la cita.");

            return RedirectToAction("Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class CitasController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
    {
        private HttpClient CrearCliente() => httpClientFactory.CreateClient();
        private string UrlApi => configuration["Valores:UrlApi"]!;

        [HttpGet]
        public async Task<IActionResult> Profesionales(int especialidadId)
        {
            using var client = CrearCliente();
            var response = await client.GetAsync($"{UrlApi}profesionales?especialidadId={especialidadId}");
            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<List<ProfesionalModel>>>();

            if (!response.IsSuccessStatusCode || resultado?.Data is null)
            {
                TempData["Mensaje"] = resultado?.Message ?? "No se pudieron cargar los profesionales.";
                return RedirectToAction("Inicio", "Home");
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
                NombrePaciente = HttpContext.Session.GetString("NombreUsuario") ?? string.Empty,
                IdentificacionPaciente = HttpContext.Session.GetString("IdentificacionUsuario") ?? string.Empty,
                CorreoPaciente = HttpContext.Session.GetString("CorreoUsuario") ?? string.Empty,
                TelefonoPaciente = HttpContext.Session.GetString("TelefonoUsuario") ?? string.Empty,
                FechaNacimientoPaciente = DateTime.TryParse(
                    HttpContext.Session.GetString("FechaNacimientoUsuario"), out var fecha) ? fecha : default
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearCitaModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            var request = new
            {
                UsuarioId = usuarioId,
                model.ProfesionalMedicoId,
                FechaHoraInicio = model.Fecha.Date + model.Hora,
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
            var resultado = await response.Content.ReadFromJsonAsync<ResultModel<List<CitaModel>>>();

            return View(resultado?.Data ?? []);
        }

        [HttpGet]
        public async Task<IActionResult> Modificar(int citaId)
        {
            using var client = CrearCliente();
            var response = await client.GetAsync($"{UrlApi}citas/{citaId}");
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
                return View(model);

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            var request = new
            {
                UsuarioId = usuarioId,
                FechaHoraInicio = model.Fecha.Date + model.Hora
            };

            using var client = CrearCliente();
            var response = await client.PutAsJsonAsync($"{UrlApi}citas/{model.CitaId}", request);

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
            var resultado = await response.Content.ReadFromJsonAsync<ResultModel>();

            TempData["Mensaje"] = resultado?.Message
                ?? (response.IsSuccessStatusCode ? "Cita cancelada." : "No se pudo cancelar la cita.");
            return RedirectToAction("Index");
        }
    }
}
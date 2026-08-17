using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using PA_WEB.Services;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class CitasController(ICitasService citasService, IProfesionalService profesionalService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> CrearAsync(
            int profesionalId,
            DateTime fecha,
            TimeSpan hora)
        {
            var profesional = await profesionalService.ObtenerProfesionalPorIdAsync(profesionalId);
            var model = new CrearCitaModel
            {
                ProfesionalMedicoId = profesionalId,
                ProfesionalMedicoNombre = profesional?.NombreCompleto ?? string.Empty,
                Fecha = fecha,
                Hora = hora,
                NombrePaciente =
                    HttpContext.Session.GetString("NombreUsuario")
                    ?? string.Empty,

                IdentificacionPaciente =
                    HttpContext.Session.GetString("IdentificacionUsuario")
                    ?? string.Empty,

                CorreoPaciente =
                    HttpContext.Session.GetString("CorreoUsuario")
                    ?? string.Empty,

                TelefonoPaciente =
                    HttpContext.Session.GetString("TelefonoUsuario")
                    ?? string.Empty,

                FechaNacimientoPaciente =
                    DateTime.TryParse(
                        HttpContext.Session.GetString("FechaNacimientoUsuario"),
                        out var fechaNacimiento)
                        ? fechaNacimiento
                        : DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CrearAsync(CrearCitaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.FechaHoraInicio.Year < 1753)
            {
                ViewBag.Mensaje = "Debe seleccionar una fecha y hora válida para la cita.";
                return View(model);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            var response = await citasService.CrearAsync(new CrearCitaModel
            {
                UsuarioId = usuarioId,
                ProfesionalMedicoId = model.ProfesionalMedicoId,
                EsParaOtraPersona = model.EsParaOtraPersona,
                NombrePaciente = model.NombrePaciente,
                IdentificacionPaciente = model.IdentificacionPaciente,
                FechaNacimientoPaciente = model.FechaNacimientoPaciente,
                CorreoPaciente = model.CorreoPaciente,
                TelefonoPaciente = model.TelefonoPaciente,
                Fecha = model.Fecha,
                Hora = model.Hora,
                Motivo = model.Motivo
            });

            if (response == null || !response.Success)
            {
                ViewBag.Mensaje = response?.Message ?? "No se pudo registrar la cita.";
                return View(model);
            }

            TempData["Mensaje"] = "Cita registrada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;
            var response = await citasService.ObtenerCitaPorUsuarioAsync(usuarioId);
            return View(response?.Data ?? []);
        }

        [HttpGet]
        public async Task<IActionResult> Modificar(
            int citaId,
            DateTime fecha,
            TimeSpan hora)
        {
            var response = await citasService.ObtenerCitaPorIdAsync(citaId);
            if (response?.Data is null)
            {
                TempData["Mensaje"] = "Cita no encontrada.";
                return RedirectToAction("Index");
            }

            var cita = response.Data;

            ViewBag.ProfesionalMedico = cita.ProfesionalMedico;

            return View(new ModificarCitaModel
            {
                NombrePaciente = cita.NombrePaciente,
                IdentificacionPaciente = cita.IdentificacionPaciente,
                Motivo = cita.Motivo,
                ProfesionalId = cita.ProfesionalMedicoId,
                CitaId = cita.Id,

                // Nueva selección
                Fecha = fecha.Date,
                Hora = hora,

                // Cita original
                FechaOriginal = cita.FechaHoraInicio.Date,
                HoraOriginal = cita.FechaHoraInicio.TimeOfDay
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

            var response = await citasService.ModificarCitaAsync(model.CitaId, usuarioId, model.Fecha.Date + model.Hora);

            if (response == null || !response.Success)
            {
                ViewBag.Mensaje = response?.Message ?? "No se pudo modificar la cita.";
                return View(model);
            }

            TempData["Mensaje"] = "Cita modificada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int citaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;

            var response = await citasService.CancelarCitaAsync(citaId, usuarioId);

            TempData["Mensaje"] = response?.Message
                ?? (response?.Success == true ? "Cita cancelada." : "No se pudo cancelar la cita.");

            return RedirectToAction("Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using PA_WEB.Services;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class ProfesionalesController(
        IProfesionalService profesionalService,
        ICitasService citaService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string? texto, int? especialidadId)
        {
            var especialidades = await profesionalService.ObtenerEspecialidadesAsync();

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

            var response = await profesionalService.BuscarProfesionalAsync(query);

            if (!response.Success || response.Data is null)
            {
                ViewBag.Mensaje = response.Message ?? "No se pudieron cargar los profesionales.";
                return View("Profesionales", new List<ProfesionalModel>());
            }

            return View("Profesionales", response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Profesionales(string? texto, int? especialidadId)
        {
            return await Index(texto, especialidadId);
        }

        [HttpGet]
        public async Task<IActionResult> Calendario(
            int profesionalId,
            DateTime? mes,
            DateTime? fecha,
            int? citaId)
        {
            var hoy = DateTime.Today;

            ProfesionalModel? profesional;
            CitaModel? cita = null;

            if (citaId.HasValue)
            {
                var response = await citaService.ObtenerCitaPorIdAsync(citaId.Value);

                if (response?.Data is null)
                {
                    TempData["Mensaje"] = "La cita no existe.";
                    return RedirectToAction("Index");
                }

                cita = response.Data;
                profesionalId = cita.ProfesionalMedicoId;
            }

            profesional = await profesionalService.ObtenerProfesionalPorIdAsync(profesionalId);

            if (profesional is null)
            {
                return NotFound();
            }

            if (cita is not null)
            {
                fecha ??= cita.FechaHoraInicio.Date;
                mes ??= cita.FechaHoraInicio;
            }

            var primerMesPermitido = new DateTime(hoy.Year, hoy.Month, 1);

            var mesActual = mes.HasValue
                ? new DateTime(mes.Value.Year, mes.Value.Month, 1)
                : primerMesPermitido;

            if (mesActual < primerMesPermitido)
            {
                mesActual = primerMesPermitido;
            }

            var primerDiaMes = mesActual;
            var ultimoDiaMes = mesActual.AddMonths(1).AddDays(-1);

            var slots = await profesionalService.ObtenerHorarioDisponiblePorProfesionalAsync(
                profesionalId,
                primerDiaMes,
                ultimoDiaMes);

            var slotsPorDia = slots
                .GroupBy(x => x.Fecha.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList());

            var diasDelMes = new List<CalendarioDiaDto>();

            for (var dia = primerDiaMes; dia <= ultimoDiaMes; dia = dia.AddDays(1))
            {
                slotsPorDia.TryGetValue(dia.Date, out var slotsDia);

                diasDelMes.Add(new CalendarioDiaDto
                {
                    Fecha = dia,
                    TieneDisponibilidad = slotsDia?.Any(x => x.Disponible) == true,
                    EsPasado = dia.Date < hoy
                });
            }

            DateTime? fechaSeleccionada = null;

            if (fecha.HasValue && fecha.Value.Date >= hoy)
            {
                fechaSeleccionada = fecha.Value.Date;
            }

            var slotsDelDia = new List<DisponibilidadSlotDto>();

            if (fechaSeleccionada.HasValue &&
                slotsPorDia.TryGetValue(fechaSeleccionada.Value, out var slotsDiaSeleccionado))
            {
                slotsDelDia = slotsDiaSeleccionado
                    .OrderBy(x => x.HoraInicio)
                    .ToList();
            }

            var model = new CalendarioViewModel
            {
                ProfesionalId = profesionalId,
                ProfesionalNombre = profesional.NombreCompleto,

                MesActual = mesActual,
                FechaSeleccionada = fechaSeleccionada,

                Dias = diasDelMes,
                Slots = slotsDelDia,

                CitaId = cita?.Id,
                FechaCitaOriginal = cita?.FechaHoraInicio.Date,
                HoraCitaOriginal = cita?.FechaHoraInicio.TimeOfDay
            };

            return View(model);
        }
    }
}
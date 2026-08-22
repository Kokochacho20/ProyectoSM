using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using PA_WEB.Services;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class MedicoController(
        IMedicoService medicoService,
        INotificacionService notificacionService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int? estadoCita)
        {
            if (!EsDoctor())
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Inicio", "Home");
            }

            var dashboard = await medicoService.ObtenerDashboardAsync();
            var citas = await medicoService.ObtenerCitasAsync(estadoCita);
            var notificaciones = await notificacionService.ObtenerNotificacionesAsync(true);

            var model = new MedicoInicioViewModel
            {
                Dashboard = dashboard?.Data ?? new MedicoDashboardModel(),
                Citas = citas?.Data ?? new List<MedicoCitaModel>(),
                Notificaciones = notificaciones?.Data ?? new List<NotificacionModel>(),
                EstadoCitaSeleccionado = estadoCita
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int citaId, int estadoCita)
        {
            if (!EsDoctor())
            {
                TempData["Mensaje"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Inicio", "Home");
            }

            var response = await medicoService.ActualizarEstadoCitaAsync(
                citaId,
                estadoCita);

            TempData["Mensaje"] = response?.Message ?? "No se pudo actualizar la cita.";

            return RedirectToAction("Index", "Medico");
        }

        [HttpPost]
        public async Task<IActionResult> MarcarNotificacionLeida(int notificacionId)
        {
            if (!EsDoctor())
            {
                TempData["Mensaje"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Inicio", "Home");
            }

            var response = await notificacionService.MarcarComoLeidaAsync(notificacionId);

            TempData["Mensaje"] = response?.Message ?? "No se pudo actualizar la notificación.";

            return RedirectToAction("Index", "Medico");
        }

        private bool EsDoctor()
        {
            return HttpContext.Session.GetInt32("RolId") == 2 &&
                   HttpContext.Session.GetInt32("ProfesionalMedicoId") is not null;
        }
    }
}
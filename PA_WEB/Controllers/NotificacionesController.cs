using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using PA_WEB.Services;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class NotificacionesController(INotificacionService notificacionService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(bool soloPendientes = false)
        {
            var response = await notificacionService.ObtenerNotificacionesAsync(soloPendientes);

            ViewBag.SoloPendientes = soloPendientes;

            if (response == null || !response.Success || response.Data is null)
            {
                ViewBag.Mensaje = response?.Message ?? "No se pudieron cargar las notificaciones.";
                return View(new List<NotificacionModel>());
            }

            return View(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int notificacionId)
        {
            var response = await notificacionService.MarcarComoLeidaAsync(notificacionId);

            TempData["Mensaje"] = response?.Message ?? "No se pudo actualizar la notificación.";

            return RedirectToAction("Index", "Notificaciones");
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var response = await notificacionService.ObtenerNotificacionesAsync(true);

            if (response?.Data != null)
            {
                foreach (var notificacion in response.Data)
                {
                    await notificacionService.MarcarComoLeidaAsync(notificacion.Id);
                }
            }

            TempData["Mensaje"] = "Notificaciones actualizadas correctamente.";

            return RedirectToAction("Index", "Notificaciones");
        }
    }
}
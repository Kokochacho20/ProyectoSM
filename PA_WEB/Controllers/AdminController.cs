using Microsoft.AspNetCore.Mvc;
using PA_WEB.Filters;
using PA_WEB.Models;
using PA_WEB.Services;

namespace PA_WEB.Controllers
{
    [RequiereSesion]
    public class AdminController(IAdminService adminService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Inicio", "Home");
            }

            var dashboard = await adminService.ObtenerDashboardAsync();
            var usuarios = await adminService.ObtenerUsuariosAsync(null, null);
            var doctores = await adminService.ObtenerDoctoresAsync();

            var model = new AdminInicioViewModel
            {
                Dashboard = dashboard?.Data ?? new AdminDashboardModel(),
                Usuarios = usuarios?.Data ?? new List<AdminUsuarioModel>(),
                Doctores = doctores?.Data ?? new List<AdminDoctorModel>()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Usuarios(string? texto, int? rolId)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Inicio", "Home");
            }

            var usuarios = await adminService.ObtenerUsuariosAsync(texto, rolId);
            var doctores = await adminService.ObtenerDoctoresAsync();

            ViewBag.Texto = texto;
            ViewBag.RolId = rolId;

            var model = new AdminInicioViewModel
            {
                Usuarios = usuarios?.Data ?? new List<AdminUsuarioModel>(),
                Doctores = doctores?.Data ?? new List<AdminDoctorModel>()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int usuarioId)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Inicio", "Home");
            }

            var usuario = await adminService.ObtenerUsuarioAsync(usuarioId);
            var doctores = await adminService.ObtenerDoctoresAsync();

            if (usuario?.Data is null)
            {
                TempData["Mensaje"] = "No se encontró el usuario solicitado.";
                return RedirectToAction("Usuarios", "Admin");
            }

            var model = new AdminEditarUsuarioViewModel
            {
                Id = usuario.Data.Id,
                Identificacion = usuario.Data.Identificacion,
                NombreCompleto = usuario.Data.NombreCompleto,
                CorreoElectronico = usuario.Data.CorreoElectronico,
                Telefono = usuario.Data.Telefono,
                FechaNacimiento = usuario.Data.FechaNacimiento,
                Estado = usuario.Data.Estado,
                RolId = usuario.Data.RolId,
                ProfesionalMedicoId = usuario.Data.ProfesionalMedicoId,
                Doctores = doctores?.Data ?? new List<AdminDoctorModel>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(AdminEditarUsuarioViewModel model)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Inicio", "Home");
            }

            if (!ModelState.IsValid)
            {
                var doctores = await adminService.ObtenerDoctoresAsync();
                model.Doctores = doctores?.Data ?? new List<AdminDoctorModel>();
                return View(model);
            }

            var response = await adminService.ActualizarUsuarioAsync(
                model.Id,
                model);

            if (response != null && response.Success)
            {
                TempData["Mensaje"] = response.Message ?? "Usuario actualizado correctamente.";
                return RedirectToAction("Usuarios", "Admin");
            }

            ViewBag.Mensaje = response?.Message ?? "No se pudo actualizar el usuario.";

            var doctoresRecargar = await adminService.ObtenerDoctoresAsync();
            model.Doctores = doctoresRecargar?.Data ?? new List<AdminDoctorModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstadoUsuario(int usuarioId, bool estado)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Inicio", "Home");
            }

            var response = await adminService.CambiarEstadoUsuarioAsync(
                usuarioId,
                estado);

            TempData["Mensaje"] = response?.Message ?? "No se pudo actualizar el estado del usuario.";

            return RedirectToAction("Usuarios", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarRolUsuario(
            int usuarioId,
            int rolId,
            int? profesionalMedicoId)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Inicio", "Home");
            }

            var request = new ActualizarRolUsuarioRequestModel
            {
                UsuarioId = usuarioId,
                RolId = rolId,
                ProfesionalMedicoId = rolId == 2 ? profesionalMedicoId : null
            };

            var response = await adminService.ActualizarRolUsuarioAsync(request);

            TempData["Mensaje"] = response?.Message ?? "No se pudo actualizar el rol del usuario.";

            return RedirectToAction("Usuarios", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> Doctores()
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Inicio", "Home");
            }

            var doctores = await adminService.ObtenerDoctoresAsync();

            return View(doctores?.Data ?? new List<AdminDoctorModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Citas(string? texto, int? estadoCita)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Inicio", "Home");
            }

            var citas = await adminService.ObtenerCitasAsync(texto, estadoCita);

            var model = new AdminCitasViewModel
            {
                Citas = citas?.Data ?? new List<AdminCitaModel>(),
                Texto = texto,
                EstadoCita = estadoCita
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstadoCita(int citaId, int estadoCita)
        {
            if (!EsSuperAdmin())
            {
                TempData["Mensaje"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Inicio", "Home");
            }

            var response = await adminService.ActualizarEstadoCitaAsync(
                citaId,
                estadoCita);

            TempData["Mensaje"] = response?.Message ?? "No se pudo actualizar el estado de la cita.";

            return RedirectToAction("Citas", "Admin");
        }

        private bool EsSuperAdmin()
        {
            return HttpContext.Session.GetInt32("RolId") == 1;
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController(IAdminService adminService, IUtilesService utilesService) : ControllerBase
    {
        [HttpPost("setup-inicial")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResultDto<SetupUsuariosInicialesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearUsuariosInicialesAsync()
        {
            var result = await adminService.CrearUsuariosInicialesAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> DashboardAsync()
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ObtenerDashboardAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> UsuariosAsync([FromQuery] string? texto, [FromQuery] int? rolId)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ObtenerUsuariosAsync(texto, rolId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("usuarios/{usuarioId}")]
        public async Task<IActionResult> ObtenerUsuarioAsync(int usuarioId)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ObtenerUsuarioAsync(usuarioId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("usuarios/{usuarioId}")]
        public async Task<IActionResult> ActualizarUsuarioAsync(
            int usuarioId,
            [FromBody] AdminActualizarUsuarioRequestDto request)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ActualizarUsuarioAsync(usuarioId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("usuarios/{usuarioId}/estado")]
        public async Task<IActionResult> CambiarEstadoUsuarioAsync(
            int usuarioId,
            [FromBody] AdminCambiarEstadoUsuarioRequestDto request)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var usuarioActualId = utilesService.ObtenerUsuarioIdToken();

            var result = await adminService.CambiarEstadoUsuarioAsync(
                usuarioId,
                request.Estado,
                usuarioActualId);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("doctores")]
        public async Task<IActionResult> DoctoresAsync()
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ObtenerDoctoresAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("usuarios/rol")]
        public async Task<IActionResult> ActualizarRolUsuarioAsync([FromBody] ActualizarRolUsuarioRequestDto request)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ActualizarRolUsuarioAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("citas")]
        public async Task<IActionResult> CitasAsync([FromQuery] string? texto, [FromQuery] int? estadoCita)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ObtenerCitasAsync(texto, estadoCita);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("citas/{citaId}/estado")]
        public async Task<IActionResult> ActualizarEstadoCitaAsync(
            int citaId,
            [FromBody] AdminActualizarEstadoCitaRequestDto request)
        {
            if (utilesService.ObtenerRolIdToken() != 1)
            {
                return Forbid();
            }

            var result = await adminService.ActualizarEstadoCitaAsync(
                citaId,
                request.EstadoCita);

            return StatusCode(result.StatusCode, result);
        }
    }
}
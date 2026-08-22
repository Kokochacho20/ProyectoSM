using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/medico")]
    [ApiController]
    [Authorize]
    public class MedicoController(IMedicoService medicoService, IUtilesService utilesService) : ControllerBase
    {
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ResultDto<MedicoDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DashboardAsync()
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();
            var rolId = utilesService.ObtenerRolIdToken();
            var profesionalMedicoId = utilesService.ObtenerProfesionalMedicoIdToken();

            if (usuarioId == 0 || rolId != 2 || profesionalMedicoId is null)
            {
                return Forbid();
            }

            var result = await medicoService.ObtenerDashboardAsync(
                usuarioId,
                profesionalMedicoId.Value);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("citas")]
        [ProducesResponseType(typeof(ResultDto<List<MedicoCitaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CitasAsync([FromQuery] int? estadoCita)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();
            var rolId = utilesService.ObtenerRolIdToken();
            var profesionalMedicoId = utilesService.ObtenerProfesionalMedicoIdToken();

            if (usuarioId == 0 || rolId != 2 || profesionalMedicoId is null)
            {
                return Forbid();
            }

            var result = await medicoService.ObtenerCitasAsync(
                profesionalMedicoId.Value,
                estadoCita);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("citas/{citaId}/estado")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarEstadoCitaAsync(
            int citaId,
            [FromBody] MedicoActualizarEstadoCitaRequestDto request)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();
            var rolId = utilesService.ObtenerRolIdToken();
            var profesionalMedicoId = utilesService.ObtenerProfesionalMedicoIdToken();

            if (usuarioId == 0 || rolId != 2 || profesionalMedicoId is null)
            {
                return Forbid();
            }

            var result = await medicoService.ActualizarEstadoCitaAsync(
                profesionalMedicoId.Value,
                citaId,
                request.EstadoCita);

            return StatusCode(result.StatusCode, result);
        }
    }
}
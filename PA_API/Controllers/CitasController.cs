using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/citas")]
    [ApiController]
    [Authorize]
    public class CitasController(ICitaMedicaService citaMedicaService, IUtilesService utilesService) : ControllerBase
    {
        [HttpPost("usuario")]
        [ProducesResponseType(typeof(ResultDto<CitaResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearCitaAsync([FromBody] CrearCitaRequestDto request)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();

            if (usuarioId == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            request.UsuarioId = usuarioId;

            var result = await citaMedicaService.CrearAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResultDto<List<CitaResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerCitasAsync()
        {
            var usuarioIdToken = utilesService.ObtenerUsuarioIdToken();

            if (usuarioIdToken == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            var result = await citaMedicaService.ObtenerCitasAsync(usuarioIdToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{citaId}")]
        [ProducesResponseType(typeof(ResultDto<CitaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerCitaPorIdAsync(int citaId)
        {
            var usuarioIdToken = utilesService.ObtenerUsuarioIdToken();

            if (usuarioIdToken == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            var result = await citaMedicaService.ObtenerCitaPorIdAsync(citaId);

            if (result.Data is not null && result.Data.UsuarioId != usuarioIdToken)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    ResultDto.Fail(StatusCodes.Status404NotFound, "Cita no encontrada."));
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{citaId}")]
        [ProducesResponseType(typeof(ResultDto<CitaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModificarCitaAsync(int citaId, [FromBody] ModificarCitaRequestDto request)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();

            if (usuarioId == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            request.UsuarioId = usuarioId;

            var result = await citaMedicaService.ModificarAsync(citaId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{citaId}/cancelar")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarCitaAsync(int citaId, [FromBody] CancelarCitaRequestDto request)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();

            if (usuarioId == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            request.UsuarioId = usuarioId;

            var result = await citaMedicaService.CancelarAsync(citaId, request.UsuarioId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
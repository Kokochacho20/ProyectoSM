using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/citas")]
    [ApiController]
    public class CitasController(ICitaMedicaService citaMedicaService) : ControllerBase
    {
        [HttpPost("usuario")]
        [ProducesResponseType(typeof(ResultDto<CitaResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearCitaAsync([FromBody] CrearCitaRequestDto request)
        {
            var result = await citaMedicaService.CrearAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResultDto<List<CitaResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerCitasAsync([FromQuery] int? usuarioId)
        {
            var result = await citaMedicaService.ObtenerCitasAsync(usuarioId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{citaId}")]
        [ProducesResponseType(typeof(ResultDto<CitaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerCitaPorIdAsync(int citaId)
        {
            var result = await citaMedicaService.ObtenerCitaPorIdAsync(citaId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{citaId}")]
        [ProducesResponseType(typeof(ResultDto<CitaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModificarCitaAsync(int citaId, [FromBody] ModificarCitaRequestDto request)
        {
            var result = await citaMedicaService.ModificarAsync(citaId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{citaId}/cancelar")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelarCitaAsync(int citaId, [FromBody] CancelarCitaRequestDto request)
        {
            var result = await citaMedicaService.CancelarAsync(citaId, request.UsuarioId);
            return StatusCode(result.StatusCode, result);
        }
    }
}

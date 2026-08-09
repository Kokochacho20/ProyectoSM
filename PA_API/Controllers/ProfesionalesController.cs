using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/profesionales")]
    [ApiController]
    [Authorize]
    public class ProfesionalesController(IProfesionalService profesionalService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ResultDto<List<ProfesionalMedicoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BuscarAsync([FromQuery] BuscarProfesionalesQueryDto query)
        {
            var result = await profesionalService.BuscarAsync(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{profesionalId}")]
        [ProducesResponseType(typeof(ResultDto<ProfesionalMedicoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerProfesionalPorId(int profesionalId)
        {
            var result = await profesionalService.ObtenerProfesionalPorIdAsync(profesionalId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
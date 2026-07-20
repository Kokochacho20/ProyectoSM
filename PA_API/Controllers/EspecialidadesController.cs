using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/especialidades")]
    [ApiController]
    public class EspecialidadesController(IEspecialidadesService especialidadesService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<EspecialidadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerEspecialidadesAsync()
        {
            var result = await especialidadesService.ObtenerEspecialidadesMedicasAsync();
            return Ok(result);
        }
    }
}

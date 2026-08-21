using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/notificaciones")]
    [ApiController]
    [Authorize]
    public class NotificacionesController(
        INotificacionService notificacionService,
        IUtilesService utilesService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ResultDto<List<NotificacionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerAsync([FromQuery] bool soloPendientes = false)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();

            if (usuarioId == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            var result = await notificacionService.ObtenerNotificacionesAsync(
                usuarioId,
                soloPendientes);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{notificacionId}/leida")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarcarLeidaAsync(int notificacionId)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();

            if (usuarioId == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            var result = await notificacionService.MarcarComoLeidaAsync(
                usuarioId,
                notificacionId);

            return StatusCode(result.StatusCode, result);
        }
    }
}
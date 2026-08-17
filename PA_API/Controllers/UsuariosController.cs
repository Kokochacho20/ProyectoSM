using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PA_API.DTOs;
using PA_API.Services;

namespace PA_API.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
    [Authorize]
    public class UsuariosController(IUsuarioService usuarioService, IUtilesService utilesService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(ResultDto<List<UsuarioDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerUsuariosAsync([FromQuery] bool? activo)
        {
            var result = await usuarioService.ObtenerUsuariosAsync(activo);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{usuarioId}")]
        [ProducesResponseType(typeof(ResultDto<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerUsuarioAsync(int usuarioId)
        {
            var result = await usuarioService.ObtenerUsuarioAsync(usuarioId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("IniciarSesion")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResultDto<InicioSesionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IniciarSesionAsync([FromBody] InicioSesionRequestDto request)
        {
            var result = await usuarioService.InicioSesionAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("RecuperarAcceso")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperarAccesoAsync([FromBody] RecuperarAccesoRequestDto request)
        {
            var result = await usuarioService.RecuperarAccesoAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("Registrar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResultDto<UsuarioDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegistrarAsync([FromBody] RegistroRequestDto request)
        {
            var result = await usuarioService.RegistrarAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("ActualizarContrasena")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarContrasenaAsync([FromBody] ActualizarContrasenaRequestDto request)
        {
            var usuarioId = utilesService.ObtenerUsuarioIdToken();

            if (usuarioId == 0)
            {
                return Unauthorized(ResultDto.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Token inválido o sesión no autorizada."));
            }

            request.UsuarioId = usuarioId;

            var result = await usuarioService.ActualizarContrasena(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
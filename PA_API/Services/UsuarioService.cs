using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using System.Data;

namespace PA_API.Services
{
    public interface IUsuarioService
    {
        Task<ResultDto<List<UsuarioDto>>> ObtenerUsuariosAsync(bool? activo);
        Task<ResultDto<UsuarioDto>> ObtenerUsuarioAsync(int usuarioId);
        Task<ResultDto<InicioSesionResponseDto>> InicioSesionAsync(InicioSesionRequestDto request);
        Task<ResultDto<UsuarioDto>> RegistrarAsync(RegistroRequestDto request);
        Task<ResultDto> RecuperarAccesoAsync(RecuperarAccesoRequestDto request);
        Task<ResultDto> ActualizarContrasena(ActualizarContrasenaRequestDto request);
    }

    public class UsuarioService(
        ILogger<UsuarioService> logger,
        IConfiguration configuration,
        IEmailService emailService,
        IUtilesService utilesService) : IUsuarioService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<ResultDto<List<UsuarioDto>>> ObtenerUsuariosAsync(bool? activo)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuarios = await conexion.QueryAsync<UsuarioDto>(
                    StoreProceduresConstants.sp_usuarios_lista,
                    new
                    {
                        Activo = activo
                    },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<UsuarioDto>>.Ok(usuarios.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuarios");
                throw;
            }
        }

        public async Task<ResultDto<UsuarioDto>> ObtenerUsuarioAsync(int usuarioId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuario = await conexion.QueryFirstOrDefaultAsync<UsuarioDto>(
                    StoreProceduresConstants.sp_usuario_obtener,
                    new
                    {
                        Id = usuarioId
                    },
                    commandType: CommandType.StoredProcedure);

                if (usuario == null)
                {
                    return ResultDto<UsuarioDto>
                        .Fail(StatusCodes.Status404NotFound, "Usuario no encontrado.");
                }

                return ResultDto<UsuarioDto>.Ok(usuario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario");
                throw;
            }
        }

        private async Task<UsuarioConContrasenaDto?> ObtenerUsuarioAsync(string correoElectronico)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuario = await conexion.QueryFirstOrDefaultAsync<UsuarioConContrasenaDto>(
                    StoreProceduresConstants.sp_usuario_iniciar_sesion,
                    new
                    {
                        CorreoElectronico = correoElectronico
                    },
                    commandType: CommandType.StoredProcedure);

                return usuario;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario por correo electrónico");
                throw;
            }
        }

        public async Task<ResultDto<InicioSesionResponseDto>> InicioSesionAsync(InicioSesionRequestDto request)
        {
            try
            {
                var usuario = await ObtenerUsuarioAsync(request.CorreoElectronico);

                if (usuario is null || !usuario.Estado)
                {
                    return ResultDto<InicioSesionResponseDto>
                        .Fail(StatusCodes.Status401Unauthorized, "No se pudo validar la sesión.");
                }

                if (usuario.TemporaryPassword)
                {
                    if (usuario.FechaExpiracionPasswordTemporal is null ||
                        usuario.FechaExpiracionPasswordTemporal.Value < DateTime.UtcNow)
                    {
                        return ResultDto<InicioSesionResponseDto>
                            .Fail(
                                StatusCodes.Status401Unauthorized,
                                "La contraseña temporal venció. Debe solicitar una nueva desde Recuperar acceso.");
                    }
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Contrasenna, usuario.PasswordHash))
                {
                    return ResultDto<InicioSesionResponseDto>
                        .Fail(StatusCodes.Status401Unauthorized, "No se pudo validar la sesión.");
                }

                var token = utilesService.GenerarToken(
                    usuario.Id,
                    usuario.Identificacion,
                    usuario.NombreCompleto,
                    usuario.CorreoElectronico);

                var expiraEn = DateTime.UtcNow.AddMinutes(30);

                var session = new InicioSesionResponseDto
                {
                    Token = token,
                    ExpiraEn = expiraEn,
                    TemporaryPassword = usuario.TemporaryPassword,
                    FechaExpiracionPasswordTemporal = usuario.FechaExpiracionPasswordTemporal,
                    Usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Identificacion = usuario.Identificacion,
                        NombreCompleto = usuario.NombreCompleto,
                        CorreoElectronico = usuario.CorreoElectronico,
                        Telefono = usuario.Telefono,
                        FechaNacimiento = usuario.FechaNacimiento
                    }
                };

                return ResultDto<InicioSesionResponseDto>.Ok(session);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al iniciar sesión de usuario");
                throw;
            }
        }

        public async Task<ResultDto<UsuarioDto>> RegistrarAsync(RegistroRequestDto request)
        {
            try
            {
                if (request.Contrasenna != request.ConfirmarContrasenna)
                {
                    return ResultDto<UsuarioDto>
                        .Fail(StatusCodes.Status400BadRequest, "Las contraseñas no coinciden.");
                }

                using IDbConnection conexion = new SqlConnection(_connectionString);

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Contrasenna);

                var parametros = new
                {
                    request.Identificacion,
                    request.NombreCompleto,
                    request.CorreoElectronico,
                    request.Telefono,
                    request.FechaNacimiento,
                    PasswordHash = passwordHash
                };

                var usuario = await conexion.QueryFirstOrDefaultAsync<UsuarioDto>(
                    StoreProceduresConstants.sp_usuario_registrar,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (usuario is null)
                {
                    return ResultDto<UsuarioDto>
                        .Fail(StatusCodes.Status400BadRequest, "Usuario no se pudo registrar.");
                }

                return ResultDto<UsuarioDto>.Ok(usuario, StatusCodes.Status201Created);
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Error SQL al registrar usuario {Correo}", request.CorreoElectronico);

                return ResultDto<UsuarioDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "Identificación o correo electrónico ya registrados.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar al nuevo usuario.");
                throw;
            }
        }

        public async Task<ResultDto> RecuperarAccesoAsync(RecuperarAccesoRequestDto request)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuario = await ObtenerUsuarioAsync(request.CorreoElectronico);

                if (usuario is null || !usuario.Estado)
                {
                    return ResultDto.Fail(
                        StatusCodes.Status400BadRequest,
                        "La información no se pudo validar correctamente.");
                }

                var passwordTemporal = GenerarPasswordTemporal();
                var nuevoHash = BCrypt.Net.BCrypt.HashPassword(passwordTemporal);
                var fechaExpiracion = DateTime.UtcNow.AddMinutes(15);

                var result = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_actualizar_contrasena,
                    new
                    {
                        usuario.Id,
                        PasswordHash = nuevoHash,
                        TemporaryPassword = true,
                        FechaExpiracionPasswordTemporal = fechaExpiracion
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == 1)
                {
                    await emailService.EnviarPasswordTemporalAsync(
                        usuario.CorreoElectronico,
                        usuario.NombreCompleto,
                        passwordTemporal,
                        fechaExpiracion);

                    return ResultDto.Ok(
                        message: "Si el correo existe, recibirá un correo con instrucciones.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    "No se ha recuperado su acceso, por favor intente nuevamente.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al recuperar acceso");
                throw;
            }
        }

        public async Task<ResultDto> ActualizarContrasena(ActualizarContrasenaRequestDto request)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                if (request.ContrasenaNueva != request.ConfirmarContrasenaNueva)
                {
                    return ResultDto.Fail(
                        StatusCodes.Status400BadRequest,
                        "Las contraseñas no coinciden.");
                }

                var nuevoHash = BCrypt.Net.BCrypt.HashPassword(request.ContrasenaNueva);

                var result = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_actualizar_contrasena,
                    new
                    {
                        request.Id,
                        PasswordHash = nuevoHash,
                        TemporaryPassword = false,
                        FechaExpiracionPasswordTemporal = (DateTime?)null
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == 1)
                {
                    return ResultDto.Ok(
                        message: "Contraseña actualizada exitosamente.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    "La contraseña temporal venció o no se pudo actualizar. Solicite una nueva recuperación de acceso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar contraseña.");
                throw;
            }
        }

        private static string GenerarPasswordTemporal()
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();

            return new string(
                Enumerable.Range(0, 10)
                    .Select(_ => caracteres[random.Next(caracteres.Length)])
                    .ToArray());
        }
    }
}
using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using System.Data;

namespace PA_API.Services
{
    public interface IAdminService
    {
        Task<ResultDto<SetupUsuariosInicialesDto>> CrearUsuariosInicialesAsync();

        Task<ResultDto<AdminDashboardDto>> ObtenerDashboardAsync();

        Task<ResultDto<List<AdminUsuarioDto>>> ObtenerUsuariosAsync(string? texto, int? rolId);

        Task<ResultDto<AdminUsuarioDto>> ObtenerUsuarioAsync(int usuarioId);

        Task<ResultDto<List<AdminDoctorDto>>> ObtenerDoctoresAsync();

        Task<ResultDto> ActualizarRolUsuarioAsync(ActualizarRolUsuarioRequestDto request);

        Task<ResultDto> ActualizarUsuarioAsync(int usuarioId, AdminActualizarUsuarioRequestDto request);

        Task<ResultDto> CambiarEstadoUsuarioAsync(int usuarioId, bool estado, int usuarioActualId);

        Task<ResultDto<List<AdminCitaDto>>> ObtenerCitasAsync(string? texto, int? estadoCita);

        Task<ResultDto> ActualizarEstadoCitaAsync(int citaId, int estadoCita);
    }

    public class AdminService(
        IConfiguration configuration,
        ILogger<AdminService> logger) : IAdminService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<ResultDto<SetupUsuariosInicialesDto>> CrearUsuariosInicialesAsync()
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuariosCreados = new List<SetupUsuarioCreadoDto>();

                const string passwordSuperAdmin = "Admin123!";
                const string passwordDoctor = "Doctor123!";

                var hashSuperAdmin = BCrypt.Net.BCrypt.HashPassword(passwordSuperAdmin);

                var superAdmin = await conexion.QueryFirstOrDefaultAsync<UsuarioDto>(
                    StoreProceduresConstants.sp_setup_crear_superadmin,
                    new
                    {
                        Identificacion = "SUPERADMIN",
                        NombreCompleto = "Super Administrador",
                        CorreoElectronico = "superadmin@sistemacitas.com",
                        Telefono = "00000000",
                        FechaNacimiento = new DateTime(2000, 1, 1),
                        PasswordHash = hashSuperAdmin
                    },
                    commandType: CommandType.StoredProcedure);

                if (superAdmin is not null)
                {
                    usuariosCreados.Add(new SetupUsuarioCreadoDto
                    {
                        UsuarioId = superAdmin.Id,
                        NombreCompleto = superAdmin.NombreCompleto,
                        CorreoElectronico = superAdmin.CorreoElectronico,
                        Rol = "SuperAdmin",
                        PasswordInicial = passwordSuperAdmin
                    });
                }

                var doctores = await conexion.QueryAsync<AdminDoctorDto>(
                    StoreProceduresConstants.sp_admin_doctores_lista,
                    commandType: CommandType.StoredProcedure);

                foreach (var doctor in doctores.Where(x => x.Estado))
                {
                    var hashDoctor = BCrypt.Net.BCrypt.HashPassword(passwordDoctor);

                    var usuarioDoctor = await conexion.QueryFirstOrDefaultAsync<UsuarioDto>(
                        StoreProceduresConstants.sp_setup_crear_usuario_doctor,
                        new
                        {
                            ProfesionalMedicoId = doctor.Id,
                            PasswordHash = hashDoctor
                        },
                        commandType: CommandType.StoredProcedure);

                    if (usuarioDoctor is not null)
                    {
                        usuariosCreados.Add(new SetupUsuarioCreadoDto
                        {
                            UsuarioId = usuarioDoctor.Id,
                            NombreCompleto = usuarioDoctor.NombreCompleto,
                            CorreoElectronico = usuarioDoctor.CorreoElectronico,
                            Rol = "Doctor",
                            PasswordInicial = passwordDoctor
                        });
                    }
                }

                var resultado = new SetupUsuariosInicialesDto
                {
                    Usuarios = usuariosCreados
                };

                return ResultDto<SetupUsuariosInicialesDto>.Ok(
                    resultado,
                    message: "Usuarios iniciales creados correctamente.");
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Error SQL al crear usuarios iniciales.");

                return ResultDto<SetupUsuariosInicialesDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear usuarios iniciales.");
                throw;
            }
        }

        public async Task<ResultDto<AdminDashboardDto>> ObtenerDashboardAsync()
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var dashboard = await conexion.QueryFirstOrDefaultAsync<AdminDashboardDto>(
                    StoreProceduresConstants.sp_admin_dashboard,
                    commandType: CommandType.StoredProcedure);

                return ResultDto<AdminDashboardDto>.Ok(
                    dashboard ?? new AdminDashboardDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener dashboard de administrador.");
                throw;
            }
        }

        public async Task<ResultDto<List<AdminUsuarioDto>>> ObtenerUsuariosAsync(string? texto, int? rolId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuarios = await conexion.QueryAsync<AdminUsuarioDto>(
                    StoreProceduresConstants.sp_admin_usuarios_lista,
                    new
                    {
                        Texto = texto,
                        RolId = rolId
                    },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<AdminUsuarioDto>>.Ok(usuarios.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuarios para administrador.");
                throw;
            }
        }

        public async Task<ResultDto<AdminUsuarioDto>> ObtenerUsuarioAsync(int usuarioId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var usuario = await conexion.QueryFirstOrDefaultAsync<AdminUsuarioDto>(
                    StoreProceduresConstants.sp_admin_usuario_obtener,
                    new
                    {
                        UsuarioId = usuarioId
                    },
                    commandType: CommandType.StoredProcedure);

                if (usuario is null)
                {
                    return ResultDto<AdminUsuarioDto>.Fail(
                        StatusCodes.Status404NotFound,
                        "Usuario no encontrado.");
                }

                return ResultDto<AdminUsuarioDto>.Ok(usuario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario para edición.");
                throw;
            }
        }

        public async Task<ResultDto<List<AdminDoctorDto>>> ObtenerDoctoresAsync()
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var doctores = await conexion.QueryAsync<AdminDoctorDto>(
                    StoreProceduresConstants.sp_admin_doctores_lista,
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<AdminDoctorDto>>.Ok(doctores.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener doctores para administrador.");
                throw;
            }
        }

        public async Task<ResultDto> ActualizarRolUsuarioAsync(ActualizarRolUsuarioRequestDto request)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var resultado = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_admin_actualizar_rol_usuario,
                    new
                    {
                        request.UsuarioId,
                        request.RolId,
                        request.ProfesionalMedicoId
                    },
                    commandType: CommandType.StoredProcedure);

                if (resultado == 1)
                {
                    return ResultDto.Ok(message: "Rol actualizado correctamente.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    "No se pudo actualizar el rol del usuario.");
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Error SQL al actualizar rol de usuario.");

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar rol de usuario.");
                throw;
            }
        }

        public async Task<ResultDto> ActualizarUsuarioAsync(int usuarioId, AdminActualizarUsuarioRequestDto request)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var resultado = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_admin_actualizar_usuario,
                    new
                    {
                        UsuarioId = usuarioId,
                        request.Identificacion,
                        request.NombreCompleto,
                        request.CorreoElectronico,
                        request.Telefono,
                        request.FechaNacimiento,
                        request.RolId,
                        request.ProfesionalMedicoId
                    },
                    commandType: CommandType.StoredProcedure);

                if (resultado >= 0)
                {
                    return ResultDto.Ok(message: "Usuario actualizado correctamente.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    "No se pudo actualizar el usuario.");
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Error SQL al actualizar usuario.");

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar usuario.");
                throw;
            }
        }

        public async Task<ResultDto> CambiarEstadoUsuarioAsync(int usuarioId, bool estado, int usuarioActualId)
        {
            try
            {
                if (usuarioId == usuarioActualId && !estado)
                {
                    return ResultDto.Fail(
                        StatusCodes.Status400BadRequest,
                        "No puede deshabilitar el usuario con el que inició sesión.");
                }

                using IDbConnection conexion = new SqlConnection(_connectionString);

                var resultado = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_admin_cambiar_estado_usuario,
                    new
                    {
                        UsuarioId = usuarioId,
                        Estado = estado
                    },
                    commandType: CommandType.StoredProcedure);

                if (resultado == 1)
                {
                    return ResultDto.Ok(
                        message: estado
                            ? "Usuario habilitado correctamente."
                            : "Usuario deshabilitado correctamente.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    "No se pudo actualizar el estado del usuario.");
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Error SQL al cambiar estado de usuario.");

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar estado de usuario.");
                throw;
            }
        }

        public async Task<ResultDto<List<AdminCitaDto>>> ObtenerCitasAsync(string? texto, int? estadoCita)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var citas = await conexion.QueryAsync<AdminCitaDto>(
                    StoreProceduresConstants.sp_admin_citas_lista,
                    new
                    {
                        Texto = texto,
                        EstadoCita = estadoCita
                    },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<AdminCitaDto>>.Ok(citas.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener citas para administrador.");
                throw;
            }
        }

        public async Task<ResultDto> ActualizarEstadoCitaAsync(int citaId, int estadoCita)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var resultado = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_admin_actualizar_estado_cita,
                    new
                    {
                        CitaId = citaId,
                        EstadoCita = estadoCita
                    },
                    commandType: CommandType.StoredProcedure);

                if (resultado == 1)
                {
                    return ResultDto.Ok(message: "Estado de cita actualizado correctamente.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    "No se pudo actualizar el estado de la cita.");
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Error SQL al actualizar estado de cita.");

                return ResultDto.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar estado de cita.");
                throw;
            }
        }
    }
}
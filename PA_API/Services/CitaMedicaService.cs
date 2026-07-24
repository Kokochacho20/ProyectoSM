using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using PA_API.Enums;
using System.Data;

namespace PA_API.Services
{
    public interface ICitaMedicaService
    {
        Task<ResultDto<CitaResponseDto>> CrearAsync(CrearCitaRequestDto request);
        Task<ResultDto<CitaResponseDto>> ObtenerCitaPorIdAsync(int citaId);
        Task<ResultDto<List<CitaResponseDto>>> ObtenerCitasAsync(int? usuarioId);
        Task<ResultDto<CitaResponseDto>> ModificarAsync(int citaId, ModificarCitaRequestDto request);
        Task<ResultDto> CancelarAsync(int citaId, int usuarioId);
    }

    public class CitaMedicaService(IConfiguration configuration, ILogger<CitaMedicaService> logger) : ICitaMedicaService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
        ?? throw new InvalidOperationException(
            $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<ResultDto<CitaResponseDto>> CrearAsync(CrearCitaRequestDto request)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var parametros = new
                {
                    request.UsuarioId,
                    request.ProfesionalMedicoId,
                    request.FechaHoraInicio,
                    FechaHoraFin = request.FechaHoraInicio.AddHours(1),
                    request.NombrePaciente,
                    request.IdentificacionPaciente,
                    request.FechaNacimientoPaciente,
                    request.CorreoPaciente,
                    request.TelefonoPaciente,
                    request.Motivo,
                    EstadoCita = (int)EstadoCita.Pendiente
                };

                var cita = await conexion.QuerySingleOrDefaultAsync<CitaResponseDto>(
                    StoreProceduresConstants.sp_crear_cita,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (cita is null)
                    return ResultDto<CitaResponseDto>.
                        Fail(StatusCodes.Status400BadRequest, "La cita no se pudo registrar correctamente.");

                return ResultDto<CitaResponseDto>.Ok(cita, StatusCodes.Status201Created);
            }
            catch (SqlException ex)
            {
                return ResultDto<CitaResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar la cita medica.");
                throw;
            }
        }

        public async Task<ResultDto<CitaResponseDto>> ObtenerCitaPorIdAsync(int citaId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var cita = await conexion.QueryFirstOrDefaultAsync<CitaResponseDto>(
                   StoreProceduresConstants.sp_obtener_cita,
                    new { Id = citaId },
                    commandType: CommandType.StoredProcedure);

                if (cita is null)
                    return ResultDto<CitaResponseDto>
                        .Fail(StatusCodes.Status404NotFound, $"Cita con id: {citaId} no encontrada.");

                return ResultDto<CitaResponseDto>.Ok(cita);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al intentar obtener cita.");
                throw;
            }
        }

        public async Task<ResultDto<List<CitaResponseDto>>> ObtenerCitasAsync(int? usuarioId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var citas = await conexion.QueryAsync<CitaResponseDto>(
                    StoreProceduresConstants.sp_obtener_citas,
                    new { UsuarioId = usuarioId },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<CitaResponseDto>>.Ok([.. citas]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al intentar obtener citas para el usuarioId: {usuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<ResultDto<CitaResponseDto>> ModificarAsync(int citaId, ModificarCitaRequestDto request)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var parametros = new
                {
                    Id = citaId,
                    request.UsuarioId,
                    request.FechaHoraInicio,
                    FechaHoraFin = request.FechaHoraInicio.AddHours(1)
                };

                var cita = await conexion.QuerySingleOrDefaultAsync<CitaResponseDto>(
                    StoreProceduresConstants.sp_modificar_cita,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (cita is null)
                    return ResultDto<CitaResponseDto>.Fail(StatusCodes.Status400BadRequest, "No se pudo modificar la cita.");

                return ResultDto<CitaResponseDto>.Ok(cita, message: "Cita modificada correctamente.");
            }
            catch (SqlException ex)
            {
                return ResultDto<CitaResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al modificar la cita con id: {citaId}", citaId);
                throw;
            }
        }

        public async Task<ResultDto> CancelarAsync(int citaId, int usuarioId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var filasAfectadas = await conexion.ExecuteAsync(
                    StoreProceduresConstants.sp_cancelar_cita,
                    new { Id = citaId, UsuarioId = usuarioId },
                    commandType: CommandType.StoredProcedure);

                if (filasAfectadas == 0)
                    return ResultDto.Fail(StatusCodes.Status400BadRequest,
                        "No se pudo cancelar la cita. Verifique que exista, le pertenezca y no esté ya cancelada o finalizada.");

                return ResultDto.Ok(message: "Cita cancelada correctamente.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cancelar la cita con id: {citaId}", citaId);
                throw;
            }
        }
    }
}
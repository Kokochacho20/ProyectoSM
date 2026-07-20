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
    }
}
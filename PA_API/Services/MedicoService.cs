using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using System.Data;

namespace PA_API.Services
{
    public interface IMedicoService
    {
        Task<ResultDto<MedicoDashboardDto>> ObtenerDashboardAsync(int usuarioId, int profesionalMedicoId);

        Task<ResultDto<List<MedicoCitaDto>>> ObtenerCitasAsync(int profesionalMedicoId, int? estadoCita);

        Task<ResultDto> ActualizarEstadoCitaAsync(int profesionalMedicoId, int citaId, int estadoCita);
    }

    public class MedicoService(
        IConfiguration configuration,
        ILogger<MedicoService> logger) : IMedicoService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<ResultDto<MedicoDashboardDto>> ObtenerDashboardAsync(int usuarioId, int profesionalMedicoId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var dashboard = await conexion.QueryFirstOrDefaultAsync<MedicoDashboardDto>(
                    StoreProceduresConstants.sp_medico_dashboard,
                    new
                    {
                        UsuarioId = usuarioId,
                        ProfesionalMedicoId = profesionalMedicoId
                    },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<MedicoDashboardDto>.Ok(
                    dashboard ?? new MedicoDashboardDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener dashboard del médico.");
                throw;
            }
        }

        public async Task<ResultDto<List<MedicoCitaDto>>> ObtenerCitasAsync(int profesionalMedicoId, int? estadoCita)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var citas = await conexion.QueryAsync<MedicoCitaDto>(
                    StoreProceduresConstants.sp_medico_citas,
                    new
                    {
                        ProfesionalMedicoId = profesionalMedicoId,
                        EstadoCita = estadoCita
                    },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<MedicoCitaDto>>.Ok(citas.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener citas del médico.");
                throw;
            }
        }

        public async Task<ResultDto> ActualizarEstadoCitaAsync(int profesionalMedicoId, int citaId, int estadoCita)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var resultado = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_medico_actualizar_estado_cita,
                    new
                    {
                        ProfesionalMedicoId = profesionalMedicoId,
                        CitaId = citaId,
                        EstadoCita = estadoCita
                    },
                    commandType: CommandType.StoredProcedure);

                if (resultado == 1)
                {
                    var mensaje = estadoCita switch
                    {
                        2 => "Cita aprobada correctamente.",
                        3 => "Cita cancelada correctamente.",
                        4 => "Cita marcada como finalizada correctamente.",
                        _ => "Estado actualizado correctamente."
                    };

                    return ResultDto.Ok(message: mensaje);
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
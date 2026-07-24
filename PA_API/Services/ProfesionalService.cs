using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using System.Data;

namespace PA_API.Services
{
    public interface IProfesionalService
    {
        Task<ResultDto<ProfesionalMedicoDto>> ObtenerProfesionalPorIdAsync(int profesionalId);
        Task<ResultDto<List<ProfesionalMedicoDto>>> BuscarAsync(BuscarProfesionalesQueryDto filtro);
    }

    public class ProfesionalService(IConfiguration configuration, ILogger<ProfesionalService> logger) : IProfesionalService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
        ?? throw new InvalidOperationException(
            $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<ResultDto<List<ProfesionalMedicoDto>>> BuscarAsync(BuscarProfesionalesQueryDto filtro)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var profesionales = (await conexion.QueryAsync<ProfesionalMedicoDto>(
                    StoreProceduresConstants.sp_profesional_buscar,
                    new { filtro.Texto, filtro.EspecialidadId },
                    commandType: CommandType.StoredProcedure)).ToList();

                return ResultDto<List<ProfesionalMedicoDto>>.Ok(profesionales);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hubo un error al buscar el profesionales con los parametros texto={texto} especialidad={expecialidad}", filtro.Texto, filtro.EspecialidadId);
                throw;
            }
        }


        public async Task<ResultDto<ProfesionalMedicoDto>> ObtenerProfesionalPorIdAsync(int profesionalId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var profesional = await conexion.QueryFirstOrDefaultAsync<ProfesionalMedicoDto>(
                    StoreProceduresConstants.sp_obtener_profesional_por_id,
                    new { profesionalId },
                    commandType: CommandType.StoredProcedure);

                if (profesional == null)
                    return ResultDto<ProfesionalMedicoDto>.Fail(StatusCodes.Status404NotFound, $"Profesional Id={profesionalId} no encontrado");

                return ResultDto<ProfesionalMedicoDto>.Ok(profesional);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hubo un error al buscar el profesional con Id={profesionalId}", profesionalId);
                throw;
            }
        }
    }
}
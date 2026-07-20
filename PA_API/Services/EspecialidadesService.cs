using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using System.Data;

namespace PA_API.Services
{
    public interface IEspecialidadesService
    {
        Task<List<EspecialidadDto>> ObtenerEspecialidadesMedicasAsync();
    }

    public class EspecialidadesService(IConfiguration configuration, ILogger<EspecialidadesService> logger) : IEspecialidadesService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
        ?? throw new InvalidOperationException(
            $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<List<EspecialidadDto>> ObtenerEspecialidadesMedicasAsync()
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var especialidades = await conexion.QueryAsync<EspecialidadDto>(
                    "sp_obtener_especialidades",
                    commandType: CommandType.StoredProcedure);
                return [.. especialidades];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hubo un error al obtener las especialidades.");
                throw;
            }
        }
    }
}

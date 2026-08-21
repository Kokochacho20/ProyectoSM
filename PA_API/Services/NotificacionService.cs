using Dapper;
using Microsoft.Data.SqlClient;
using PA_API.Constants;
using PA_API.DTOs;
using System.Data;

namespace PA_API.Services
{
    public interface INotificacionService
    {
        Task<ResultDto<List<NotificacionDto>>> ObtenerNotificacionesAsync(int usuarioId, bool soloPendientes);

        Task<ResultDto> MarcarComoLeidaAsync(int usuarioId, int notificacionId);
    }

    public class NotificacionService(
        IConfiguration configuration,
        ILogger<NotificacionService> logger) : INotificacionService
    {
        private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringConstants.MainDatabase)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringConstants.MainDatabase}' no configurada.");

        public async Task<ResultDto<List<NotificacionDto>>> ObtenerNotificacionesAsync(int usuarioId, bool soloPendientes)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var notificaciones = await conexion.QueryAsync<NotificacionDto>(
                    StoreProceduresConstants.sp_notificaciones_usuario,
                    new
                    {
                        UsuarioId = usuarioId,
                        SoloPendientes = soloPendientes
                    },
                    commandType: CommandType.StoredProcedure);

                return ResultDto<List<NotificacionDto>>.Ok(notificaciones.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener notificaciones.");
                throw;
            }
        }

        public async Task<ResultDto> MarcarComoLeidaAsync(int usuarioId, int notificacionId)
        {
            try
            {
                using IDbConnection conexion = new SqlConnection(_connectionString);

                var resultado = await conexion.QueryFirstOrDefaultAsync<int>(
                    StoreProceduresConstants.sp_notificacion_marcar_leida,
                    new
                    {
                        UsuarioId = usuarioId,
                        NotificacionId = notificacionId
                    },
                    commandType: CommandType.StoredProcedure);

                if (resultado == 1)
                {
                    return ResultDto.Ok(message: "Notificación marcada como leída.");
                }

                return ResultDto.Fail(
                    StatusCodes.Status404NotFound,
                    "Notificación no encontrada.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al marcar notificación como leída.");
                throw;
            }
        }
    }
}
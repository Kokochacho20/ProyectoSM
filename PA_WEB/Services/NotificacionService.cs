using PA_WEB.Models;

namespace PA_WEB.Services
{
    public interface INotificacionService
    {
        Task<ResultModel<List<NotificacionModel>>?> ObtenerNotificacionesAsync(bool soloPendientes);

        Task<ResultModel?> MarcarComoLeidaAsync(int notificacionId);
    }

    public class NotificacionService(
        IHttpClientFactory httpClientFactory,
        ILogger<NotificacionService> logger) : INotificacionService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<ResultModel<List<NotificacionModel>>?> ObtenerNotificacionesAsync(bool soloPendientes)
        {
            try
            {
                var response = await _client.GetAsync($"notificaciones?soloPendientes={soloPendientes}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResultModel<List<NotificacionModel>>
                    {
                        Success = false,
                        Message = "No se pudieron cargar las notificaciones.",
                        Data = new List<NotificacionModel>()
                    };
                }

                return await response.Content.ReadFromJsonAsync<ResultModel<List<NotificacionModel>>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener notificaciones.");

                return new ResultModel<List<NotificacionModel>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las notificaciones.",
                    Data = new List<NotificacionModel>()
                };
            }
        }

        public async Task<ResultModel?> MarcarComoLeidaAsync(int notificacionId)
        {
            try
            {
                var response = await _client.PutAsync(
                    $"notificaciones/{notificacionId}/leida",
                    null);

                return await response.Content.ReadFromJsonAsync<ResultModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al marcar notificación como leída.");

                return new ResultModel
                {
                    Success = false,
                    Message = "No se pudo marcar la notificación como leída."
                };
            }
        }
    }
}
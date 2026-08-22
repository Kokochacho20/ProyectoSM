using PA_WEB.Models;

namespace PA_WEB.Services
{
    public interface IMedicoService
    {
        Task<ResultModel<MedicoDashboardModel>?> ObtenerDashboardAsync();
        Task<ResultModel<List<MedicoCitaModel>>?> ObtenerCitasAsync(int? estadoCita);
        Task<ResultModel?> ActualizarEstadoCitaAsync(int citaId, int estadoCita);
    }

    public class MedicoService(
        IHttpClientFactory httpClientFactory,
        ILogger<MedicoService> logger) : IMedicoService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<ResultModel<MedicoDashboardModel>?> ObtenerDashboardAsync()
        {
            try
            {
                var response = await _client.GetAsync("medico/dashboard");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<MedicoDashboardModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener dashboard del médico.");

                return new ResultModel<MedicoDashboardModel>
                {
                    Success = false,
                    Message = "No se pudo cargar el dashboard del médico.",
                    Data = new MedicoDashboardModel()
                };
            }
        }

        public async Task<ResultModel<List<MedicoCitaModel>>?> ObtenerCitasAsync(int? estadoCita)
        {
            try
            {
                var query = estadoCita.HasValue && estadoCita.Value > 0
                    ? $"?estadoCita={estadoCita.Value}"
                    : string.Empty;

                var response = await _client.GetAsync($"medico/citas{query}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<List<MedicoCitaModel>>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener citas del médico.");

                return new ResultModel<List<MedicoCitaModel>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las citas del médico.",
                    Data = new List<MedicoCitaModel>()
                };
            }
        }

        public async Task<ResultModel?> ActualizarEstadoCitaAsync(int citaId, int estadoCita)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(
                    $"medico/citas/{citaId}/estado",
                    new
                    {
                        EstadoCita = estadoCita
                    });

                return await response.Content.ReadFromJsonAsync<ResultModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar estado de cita.");

                return new ResultModel
                {
                    Success = false,
                    Message = "No se pudo actualizar el estado de la cita."
                };
            }
        }
    }
}
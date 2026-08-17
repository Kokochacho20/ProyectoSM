using PA_WEB.Models;

namespace PA_WEB.Services
{
    public interface ICitasService
    {
        Task<ResultModel<CitaModel>?> CrearAsync(CrearCitaModel payload);
        Task<ResultModel<List<CitaModel>>?> ObtenerCitaPorUsuarioAsync(int usuarioId);
        Task<ResultModel<CitaModel>?> ObtenerCitaPorIdAsync(int citaId);
        Task<ResultModel<CitaModel>?> ModificarCitaAsync(int citaId, int usuarioId, DateTime fechaHoraInicio);
        Task<ResultModel<CitaModel>?> CancelarCitaAsync(int citaId, int usuarioId);
    }

    public class CitasService(
        IHttpClientFactory httpClientFactory,
        ILogger<CitasService> logger) : ICitasService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<ResultModel<CitaModel>?> CrearAsync(CrearCitaModel payload)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("citas/usuario", payload);
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al crear la cita.");
                return new ResultModel<CitaModel>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al crear la cita.",
                    Data = null
                };
            }
        }

        public async Task<ResultModel<List<CitaModel>>?> ObtenerCitaPorUsuarioAsync(int usuarioId)
        {
            try
            {
                var response = await _client.GetAsync($"citas?usuarioId={usuarioId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<List<CitaModel>>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al obtener las citas del usuario {UsuarioId}.", usuarioId);
                return new ResultModel<List<CitaModel>>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al obtener las citas del usuario.",
                    Data = []
                };
            }
        }

        public async Task<ResultModel<CitaModel>?> ObtenerCitaPorIdAsync(int citaId)
        {
            try
            {
                var response = await _client.GetAsync($"citas/{citaId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al obtener la cita {CitaId}.", citaId);
                return new ResultModel<CitaModel>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al obtener la cita.",
                    Data = null
                };
            }
        }

        public async Task<ResultModel<CitaModel>?> ModificarCitaAsync(int citaId, int usuarioId, DateTime fechaHoraInicio)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"citas/{citaId}", new { usuarioId, fechaHoraInicio });
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al modificar la cita {CitaId}.", citaId);
                return new ResultModel<CitaModel>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al modificar la cita.",
                    Data = null
                };
            }
        }

        public async Task<ResultModel<CitaModel>?> CancelarCitaAsync(int citaId, int usuarioId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"citas/{citaId}/cancelar")
                {
                    Content = JsonContent.Create(new { UsuarioId = usuarioId })
                };

                var response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<CitaModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al cancelar la cita {CitaId}.", citaId);
                return new ResultModel<CitaModel>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al cancelar la cita.",
                    Data = null
                };
            }
        }
    }
}
using PA_WEB.Models;

namespace PA_WEB.Services
{
    public interface IProfesionalService
    {
        Task<List<EspecialidadModel>> ObtenerEspecialidadesAsync();
        Task<ProfesionalModel?> ObtenerProfesionalPorIdAsync(int profesionalId);
        Task<ResultModel<List<ProfesionalModel>>> BuscarProfesionalAsync(string query);
        Task<List<DisponibilidadSlotDto>> ObtenerHorarioDisponiblePorProfesionalAsync(
            int profesionalId,
            DateTime primerDiaMes,
            DateTime ultimoDiaMes);
    }

    public class ProfesionalService(
        IHttpClientFactory httpClientFactory,
        ILogger<ProfesionalService> logger) : IProfesionalService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<List<EspecialidadModel>> ObtenerEspecialidadesAsync()
        {
            try
            {
                var response = await _client.GetAsync("especialidades");

                if (!response.IsSuccessStatusCode) return [];

                return await response.Content
                    .ReadFromJsonAsync<List<EspecialidadModel>>() ?? [];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al obtener especialidades.");
                return [];
            }
        }

        public async Task<ProfesionalModel?> ObtenerProfesionalPorIdAsync(int profesionalId)
        {
            try
            {
                var response = await _client.GetAsync(
                    $"profesionales/{profesionalId}");

                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<ResultModel<ProfesionalModel>>();

                return result?.Data;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Ha ocurrido un error al buscar el profesional {ProfesionalId}.",
                    profesionalId);

                return null;
            }
        }

        public async Task<ResultModel<List<ProfesionalModel>>> BuscarProfesionalAsync(
            string query)
        {
            try
            {
                var response = await _client.GetAsync($"profesionales{query}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ResultModel<List<ProfesionalModel>>
                    {
                        Success = false,
                        Data = []
                    };
                }

                return await response.Content
                    .ReadFromJsonAsync<ResultModel<List<ProfesionalModel>>>()
                    ?? new ResultModel<List<ProfesionalModel>>
                    {
                        Success = false,
                        Data = []
                    };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Ha ocurrido un error al buscar profesionales.");

                return new ResultModel<List<ProfesionalModel>>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al buscar profesionales.",
                    Data = []
                };
            }
        }

        public async Task<List<DisponibilidadSlotDto>>
            ObtenerHorarioDisponiblePorProfesionalAsync(
                int profesionalId,
                DateTime primerDiaMes,
                DateTime ultimoDiaMes)
        {
            try
            {
                var url =
                    $"profesionales/{profesionalId}/disponibilidad" +
                    $"?inicio={primerDiaMes:yyyy-MM-dd}" +
                    $"&fin={ultimoDiaMes:yyyy-MM-dd}";

                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return [];

                return await response.Content
                    .ReadFromJsonAsync<List<DisponibilidadSlotDto>>() ?? [];
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Ha ocurrido un error al obtener disponibilidad del profesional {ProfesionalId}.",
                    profesionalId);

                return [];
            }
        }
    }
}
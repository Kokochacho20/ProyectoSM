using PA_WEB.Models;
using System.Net.Http.Json;

namespace PA_WEB.Services
{
    public interface IAdminService
    {
        Task<ResultModel<AdminDashboardModel>?> ObtenerDashboardAsync();
        Task<ResultModel<List<AdminUsuarioModel>>?> ObtenerUsuariosAsync(string? texto, int? rolId);
        Task<ResultModel<AdminUsuarioModel>?> ObtenerUsuarioAsync(int usuarioId);
        Task<ResultModel<List<AdminDoctorModel>>?> ObtenerDoctoresAsync();
        Task<ResultModel?> ActualizarRolUsuarioAsync(ActualizarRolUsuarioRequestModel request);
        Task<ResultModel?> ActualizarUsuarioAsync(int usuarioId, AdminEditarUsuarioViewModel model);
        Task<ResultModel?> CambiarEstadoUsuarioAsync(int usuarioId, bool estado);

        Task<ResultModel<List<AdminCitaModel>>?> ObtenerCitasAsync(string? texto, int? estadoCita);
        Task<ResultModel?> ActualizarEstadoCitaAsync(int citaId, int estadoCita);
    }

    public class AdminService(
        IHttpClientFactory httpClientFactory,
        ILogger<AdminService> logger) : IAdminService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<ResultModel<AdminDashboardModel>?> ObtenerDashboardAsync()
        {
            try
            {
                var response = await _client.GetAsync("admin/dashboard");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<AdminDashboardModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener dashboard de administrador.");

                return new ResultModel<AdminDashboardModel>
                {
                    Success = false,
                    Message = "No se pudo cargar el dashboard de administrador.",
                    Data = new AdminDashboardModel()
                };
            }
        }

        public async Task<ResultModel<List<AdminUsuarioModel>>?> ObtenerUsuariosAsync(string? texto, int? rolId)
        {
            try
            {
                var parametros = new List<string>();

                if (!string.IsNullOrWhiteSpace(texto))
                    parametros.Add($"texto={Uri.EscapeDataString(texto)}");

                if (rolId.HasValue && rolId.Value > 0)
                    parametros.Add($"rolId={rolId.Value}");

                var query = parametros.Count > 0
                    ? "?" + string.Join("&", parametros)
                    : string.Empty;

                var response = await _client.GetAsync($"admin/usuarios{query}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<List<AdminUsuarioModel>>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuarios de administrador.");

                return new ResultModel<List<AdminUsuarioModel>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los usuarios.",
                    Data = new List<AdminUsuarioModel>()
                };
            }
        }

        public async Task<ResultModel<AdminUsuarioModel>?> ObtenerUsuarioAsync(int usuarioId)
        {
            try
            {
                var response = await _client.GetAsync($"admin/usuarios/{usuarioId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<AdminUsuarioModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario.");

                return new ResultModel<AdminUsuarioModel>
                {
                    Success = false,
                    Message = "No se pudo cargar la información del usuario.",
                    Data = new AdminUsuarioModel()
                };
            }
        }

        public async Task<ResultModel<List<AdminDoctorModel>>?> ObtenerDoctoresAsync()
        {
            try
            {
                var response = await _client.GetAsync("admin/doctores");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<List<AdminDoctorModel>>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener doctores de administrador.");

                return new ResultModel<List<AdminDoctorModel>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los doctores.",
                    Data = new List<AdminDoctorModel>()
                };
            }
        }

        public async Task<ResultModel?> ActualizarRolUsuarioAsync(ActualizarRolUsuarioRequestModel request)
        {
            try
            {
                var response = await _client.PutAsJsonAsync("admin/usuarios/rol", request);

                return await response.Content.ReadFromJsonAsync<ResultModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar rol de usuario.");

                return new ResultModel
                {
                    Success = false,
                    Message = "No se pudo actualizar el rol del usuario."
                };
            }
        }

        public async Task<ResultModel?> ActualizarUsuarioAsync(int usuarioId, AdminEditarUsuarioViewModel model)
        {
            try
            {
                var request = new
                {
                    model.Identificacion,
                    model.NombreCompleto,
                    model.CorreoElectronico,
                    model.Telefono,
                    model.FechaNacimiento,
                    model.RolId,
                    ProfesionalMedicoId = model.RolId == 2 ? model.ProfesionalMedicoId : null
                };

                var response = await _client.PutAsJsonAsync(
                    $"admin/usuarios/{usuarioId}",
                    request);

                return await response.Content.ReadFromJsonAsync<ResultModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar usuario.");

                return new ResultModel
                {
                    Success = false,
                    Message = "No se pudo actualizar el usuario."
                };
            }
        }

        public async Task<ResultModel?> CambiarEstadoUsuarioAsync(int usuarioId, bool estado)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(
                    $"admin/usuarios/{usuarioId}/estado",
                    new
                    {
                        Estado = estado
                    });

                return await response.Content.ReadFromJsonAsync<ResultModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar estado de usuario.");

                return new ResultModel
                {
                    Success = false,
                    Message = "No se pudo actualizar el estado del usuario."
                };
            }
        }

        public async Task<ResultModel<List<AdminCitaModel>>?> ObtenerCitasAsync(string? texto, int? estadoCita)
        {
            try
            {
                var parametros = new List<string>();

                if (!string.IsNullOrWhiteSpace(texto))
                    parametros.Add($"texto={Uri.EscapeDataString(texto)}");

                if (estadoCita.HasValue && estadoCita.Value > 0)
                    parametros.Add($"estadoCita={estadoCita.Value}");

                var query = parametros.Count > 0
                    ? "?" + string.Join("&", parametros)
                    : string.Empty;

                var response = await _client.GetAsync($"admin/citas{query}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ResultModel<List<AdminCitaModel>>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener citas de administrador.");

                return new ResultModel<List<AdminCitaModel>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las citas.",
                    Data = new List<AdminCitaModel>()
                };
            }
        }

        public async Task<ResultModel?> ActualizarEstadoCitaAsync(int citaId, int estadoCita)
        {
            try
            {
                var response = await _client.PutAsJsonAsync(
                    $"admin/citas/{citaId}/estado",
                    new AdminActualizarEstadoCitaRequestModel
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
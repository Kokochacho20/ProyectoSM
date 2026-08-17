using PA_WEB.Models;

namespace PA_WEB.Services
{
    public interface IUsuarioService
    {
        Task<ResultModel<InicioSesionResponseModel>?> IniciarSesionAsync(string correoElectronico, string contrasenna);
        Task<ResultModel<UsuarioModel>?> RegistrarUsuarioAsync(RegistrarUsuarioModel payload);
        Task<ResultModel?> RecuperarAccesoAsync(string correoElectronico);
        Task<ResultModel?> ActualizarContrasennaAsync(string contrasenaNueva, string confirmarContrasenaNueva);
    }

    public class UsuariosService(IHttpClientFactory httpClientFactory, ILogger<UsuariosService> logger) : IUsuarioService
    {
        readonly HttpClient _client = httpClientFactory.CreateClient("ApiClient");

        public async Task<ResultModel<InicioSesionResponseModel>?> IniciarSesionAsync(string correoElectronico, string contrasenna)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("usuarios/IniciarSesion", new
                {
                    correoElectronico,
                    contrasenna
                });

                var resultado = await response.Content.ReadFromJsonAsync<ResultModel<InicioSesionResponseModel>>();
                return resultado;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al iniciar sesion.");
                return new ResultModel<InicioSesionResponseModel> { 
                    Success = false, 
                    Message = "Ha ocurrido un error al iniciar sesión." 
                };
            }
        }

        public async Task<ResultModel<UsuarioModel>?> RegistrarUsuarioAsync(RegistrarUsuarioModel payload)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("usuarios/Registrar", payload);
                return await response.Content.ReadFromJsonAsync<ResultModel<UsuarioModel>>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al registrar usuario.");
                return new ResultModel<UsuarioModel>
                {
                    Success = false,
                    Message = "Ha ocurrido un error al registrar usuario."
                };
            }
        }

        public async Task<ResultModel?> RecuperarAccesoAsync(string correoElectronico)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("usuarios/RecuperarAcceso", new
                {
                    correoElectronico
                });

                return await response.Content.ReadFromJsonAsync<ResultModel?>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al intentar recuperar acceso.");
                return new ResultModel
                {
                    Success = false,
                    Message = "Ha ocurrido un error al intentar recuperar acceso."
                };
            }
        }

        public async Task<ResultModel?> ActualizarContrasennaAsync(string contrasenaNueva, string confirmarContrasenaNueva)
        {
            try
            {
                var response = await _client.PutAsJsonAsync("usuarios/ActualizarContrasena", new
                {
                    contrasenaNueva,
                    confirmarContrasenaNueva
                });

                return await response.Content.ReadFromJsonAsync<ResultModel?>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido un error al intentar actualizar contrasenna.");
                return new ResultModel
                {
                    Success = false,
                    Message = "Ha ocurrido un error al intentar actualizar la contraseña."
                };
            }
        }
    }
}

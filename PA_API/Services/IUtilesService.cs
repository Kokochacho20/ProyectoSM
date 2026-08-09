namespace PA_API.Services
{
    public interface IUtilesService
    {
        string GenerarToken(int usuarioId, string identificacion, string nombreCompleto, string correoElectronico);

        int ObtenerUsuarioIdToken();

        string ObtenerCorreoToken();

        string ObtenerNombreToken();
    }
}
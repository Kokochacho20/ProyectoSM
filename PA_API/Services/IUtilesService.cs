namespace PA_API.Services
{
    public interface IUtilesService
    {
        string GenerarToken(
            int usuarioId,
            string identificacion,
            string nombreCompleto,
            string correoElectronico,
            int rolId,
            string rolNombre,
            int? profesionalMedicoId);

        int ObtenerUsuarioIdToken();

        string ObtenerCorreoToken();

        string ObtenerNombreToken();

        int ObtenerRolIdToken();

        int? ObtenerProfesionalMedicoIdToken();
    }
}
namespace PA_API.Services
{
    public interface IEmailService
    {
        Task EnviarPasswordTemporalAsync(string correoDestino, string nombreUsuario, string passwordTemporal);
    }
    public class EmailService(ILogger<EmailService> logger) : IEmailService
    {
        public async Task EnviarPasswordTemporalAsync(string correoDestino, string nombreUsuario, string passwordTemporal)
        {
            logger.LogInformation("Enviando correo a {correoDestino} con password temporal para usuario {nombreUsuario}", correoDestino, nombreUsuario);
            await Task.CompletedTask;
        }
    }
}

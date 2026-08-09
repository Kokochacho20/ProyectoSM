using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace PA_API.Services
{
    public interface IEmailService
    {
        Task EnviarPasswordTemporalAsync(
            string correoDestino,
            string nombreUsuario,
            string passwordTemporal,
            DateTime fechaExpiracion);
    }

    public class EmailService(
        ILogger<EmailService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment) : IEmailService
    {
        public async Task EnviarPasswordTemporalAsync(
            string correoDestino,
            string nombreUsuario,
            string passwordTemporal,
            DateTime fechaExpiracion)
        {
            try
            {
                var nombreRemitente = configuration["Correos:NombreRemitente"] ?? "Clínica SM";
                var correoOrigen = configuration["Correos:Correo"];
                var appPassword = configuration["Correos:AppPassword"];
                var smtpHost = configuration["Correos:SmtpHost"] ?? "smtp.gmail.com";

                var smtpPort = int.TryParse(configuration["Correos:SmtpPort"], out var puerto)
                    ? puerto
                    : 587;

                if (string.IsNullOrWhiteSpace(correoOrigen) || string.IsNullOrWhiteSpace(appPassword))
                {
                    logger.LogWarning("No se envió el correo porque no está configurado Correos:Correo o Correos:AppPassword.");
                    return;
                }

                var cuerpoHtml = await ObtenerPlantillaRecuperarAccesoAsync();

                cuerpoHtml = cuerpoHtml.Replace("{{NOMBRE}}", nombreUsuario);
                cuerpoHtml = cuerpoHtml.Replace("{{TEMPORAL}}", passwordTemporal);
                cuerpoHtml = cuerpoHtml.Replace("{{CORREO}}", correoDestino);
                cuerpoHtml = cuerpoHtml.Replace("{{FECHA}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                cuerpoHtml = cuerpoHtml.Replace("{{EXPIRA}}", fechaExpiracion.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));

                var mensaje = new MimeMessage();

                mensaje.From.Add(new MailboxAddress(nombreRemitente, correoOrigen));
                mensaje.To.Add(MailboxAddress.Parse(correoDestino));
                mensaje.Subject = "Recuperación de acceso - Clínica SM";

                mensaje.Body = new TextPart(TextFormat.Html)
                {
                    Text = cuerpoHtml
                };

                using var cliente = new SmtpClient();

                await cliente.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

                await cliente.AuthenticateAsync(
                    correoOrigen,
                    appPassword.Replace(" ", string.Empty));

                await cliente.SendAsync(mensaje);

                await cliente.DisconnectAsync(true);

                logger.LogInformation("Correo de recuperación enviado correctamente a {correoDestino}", correoDestino);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al enviar correo de recuperación a {correoDestino}", correoDestino);
                throw;
            }
        }

        private async Task<string> ObtenerPlantillaRecuperarAccesoAsync()
        {
            var rutaPlantilla = Path.Combine(
                environment.ContentRootPath,
                "Templates",
                "RecuperarAcceso.html");

            if (File.Exists(rutaPlantilla))
            {
                return await File.ReadAllTextAsync(rutaPlantilla);
            }

            return """
                   <!DOCTYPE html>
                   <html lang="es">
                   <head>
                       <meta charset="UTF-8">
                       <title>Recuperación de acceso</title>
                   </head>
                   <body style="font-family: Arial, sans-serif; background-color:#f5f6fa; padding: 20px;">
                       <div style="max-width: 600px; margin:auto; background:#ffffff; padding:25px; border-radius:10px;">
                           <h2 style="color:#5b5ce2;">Clínica SM</h2>
                           <p>Hola {{NOMBRE}},</p>
                           <p>Se solicitó la recuperación de acceso para su cuenta.</p>
                           <p>Su contraseña temporal es:</p>
                           <h3 style="background:#eef0ff; padding:12px; border-radius:8px;">{{TEMPORAL}}</h3>
                           <p>Esta contraseña temporal vence el: <strong>{{EXPIRA}}</strong></p>
                           <p>Ingrese al sistema con esta contraseña temporal y luego actualice su contraseña desde el menú de cuenta.</p>
                           <p style="font-size:12px; color:#777;">Fecha de solicitud: {{FECHA}}</p>
                       </div>
                   </body>
                   </html>
                   """;
        }
    }
}
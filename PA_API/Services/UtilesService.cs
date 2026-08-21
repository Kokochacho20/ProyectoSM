using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PA_API.Services
{
    public class UtilesService(IConfiguration configuration, IHttpContextAccessor httpContext) : IUtilesService
    {
        public string GenerarToken(
            int usuarioId,
            string identificacion,
            string nombreCompleto,
            string correoElectronico,
            int rolId,
            string rolNombre,
            int? profesionalMedicoId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var secretKey = configuration["Jwt:SecretKey"]!;

            var timeoutValido = int.TryParse(configuration["Jwt:Timeout"], out var timeout);
            var minutosExpiracion = timeoutValido && timeout > 0 ? timeout : 30;

            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim("usuarioId", usuarioId.ToString()),
                new Claim("identificacion", identificacion),
                new Claim("nombreCompleto", nombreCompleto),
                new Claim("correoElectronico", correoElectronico),
                new Claim("rolId", rolId.ToString()),
                new Claim("rolNombre", rolNombre)
            };

            if (profesionalMedicoId.HasValue)
            {
                claims.Add(new Claim("profesionalMedicoId", profesionalMedicoId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(minutosExpiracion),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public int ObtenerUsuarioIdToken()
        {
            var valor = httpContext.HttpContext?.User.FindFirstValue("usuarioId");

            return int.TryParse(valor, out var usuarioId) ? usuarioId : 0;
        }

        public string ObtenerCorreoToken()
        {
            var valor = httpContext.HttpContext?.User.FindFirstValue("correoElectronico");

            return valor ?? string.Empty;
        }

        public string ObtenerNombreToken()
        {
            var valor = httpContext.HttpContext?.User.FindFirstValue("nombreCompleto");

            return valor ?? string.Empty;
        }

        public int ObtenerRolIdToken()
        {
            var valor = httpContext.HttpContext?.User.FindFirstValue("rolId");

            return int.TryParse(valor, out var rolId) ? rolId : 0;
        }

        public int? ObtenerProfesionalMedicoIdToken()
        {
            var valor = httpContext.HttpContext?.User.FindFirstValue("profesionalMedicoId");

            return int.TryParse(valor, out var profesionalMedicoId)
                ? profesionalMedicoId
                : null;
        }
    }
}
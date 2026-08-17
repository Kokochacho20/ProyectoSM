using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PA_API.Services
{
    public class UtilesService(IConfiguration configuration, IHttpContextAccessor httpContext) : IUtilesService
    {
        public string GenerarToken(int usuarioId, string identificacion, string nombreCompleto, string correoElectronico)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var secretKey = configuration["Jwt:SecretKey"]!;
            _ = int.TryParse(configuration["Jwt:Timeout"]!, out int timeout);

            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new[]
            {
                new Claim("usuarioId", usuarioId.ToString()),
                new Claim("identificacion", identificacion),
                new Claim("nombreCompleto", nombreCompleto),
                new Claim("correoElectronico", correoElectronico)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(timeout),
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
    }
}
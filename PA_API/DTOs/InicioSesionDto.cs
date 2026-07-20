using System.ComponentModel.DataAnnotations;

namespace PA_API.DTOs
{
    public class InicioSesionRequestDto
    {
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
    }

    public class InicioSesionResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public UsuarioDto Usuario { get; set; } = default!;
    }
}

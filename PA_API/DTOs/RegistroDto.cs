using System.ComponentModel.DataAnnotations;

namespace PA_API.DTOs
{
    public class RegistroRequestDto
    {
        [Required]
        public string Identificacion { get; set; } = string.Empty;
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        [Required]
        public DateTime FechaNacimiento { get; set; }
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
        [Required]
        public string ConfirmarContrasenna { get; set; } = string.Empty;
    }

    public class RecuperarAccesoRequestDto
    {
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
    }

    public class ActualizarContrasenaRequestDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string ContrasenaNueva { get; set; } = string.Empty;
        [Required]
        public string ConfirmarContrasenaNueva { get; set; } = string.Empty;
    }
}
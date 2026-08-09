namespace PA_API.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }

        public string Identificacion { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }
    }

    public class UsuarioConContrasenaDto
    {
        public int Id { get; set; }

        public string Identificacion { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public bool TemporaryPassword { get; set; }

        public DateTime? FechaExpiracionPasswordTemporal { get; set; }

        public DateTime FechaRegistro { get; set; }

        public bool Estado { get; set; }
    }
}
namespace PA_API.DTOs
{
    public class AdminDashboardDto
    {
        public int UsuariosRegistrados { get; set; }
        public int Administradores { get; set; }
        public int UsuariosDoctores { get; set; }
        public int Doctores { get; set; }
        public int Especialidades { get; set; }
        public int CitasAgendadas { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasAprobadas { get; set; }
        public int CitasCanceladas { get; set; }
        public int CitasFinalizadas { get; set; }
    }

    public class AdminUsuarioDto
    {
        public int Id { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public bool Estado { get; set; }
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public int? ProfesionalMedicoId { get; set; }
        public string? ProfesionalNombre { get; set; }
    }

    public class AdminDoctorDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CodigoMedico { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioConsulta { get; set; }
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public int? UsuarioId { get; set; }
        public string? CorreoUsuario { get; set; }
        public bool TieneUsuario { get; set; }
    }

    public class AdminCitaDto
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public string UsuarioRegistro { get; set; } = string.Empty;
        public string CorreoUsuarioRegistro { get; set; } = string.Empty;

        public int ProfesionalMedicoId { get; set; }
        public string ProfesionalMedico { get; set; } = string.Empty;
        public string CorreoProfesional { get; set; } = string.Empty;

        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }

        public string NombrePaciente { get; set; } = string.Empty;
        public string IdentificacionPaciente { get; set; } = string.Empty;
        public DateTime FechaNacimientoPaciente { get; set; }
        public string CorreoPaciente { get; set; } = string.Empty;
        public string TelefonoPaciente { get; set; } = string.Empty;
        public string? Motivo { get; set; }

        public int EstadoCita { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class ActualizarRolUsuarioRequestDto
    {
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
        public int? ProfesionalMedicoId { get; set; }
    }

    public class AdminActualizarUsuarioRequestDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public int RolId { get; set; }
        public int? ProfesionalMedicoId { get; set; }
    }

    public class AdminCambiarEstadoUsuarioRequestDto
    {
        public bool Estado { get; set; }
    }

    public class AdminActualizarEstadoCitaRequestDto
    {
        public int EstadoCita { get; set; }
    }

    public class SetupUsuariosInicialesDto
    {
        public List<SetupUsuarioCreadoDto> Usuarios { get; set; } = new();
    }

    public class SetupUsuarioCreadoDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string PasswordInicial { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace PA_WEB.Models
{
    public class AdminDashboardModel
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

    public class AdminUsuarioModel
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

    public class AdminDoctorModel
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

    public class AdminCitaModel
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

    public class AdminInicioViewModel
    {
        public AdminDashboardModel Dashboard { get; set; } = new();
        public List<AdminUsuarioModel> Usuarios { get; set; } = new();
        public List<AdminDoctorModel> Doctores { get; set; } = new();
    }

    public class AdminCitasViewModel
    {
        public List<AdminCitaModel> Citas { get; set; } = new();
        public string? Texto { get; set; }
        public int? EstadoCita { get; set; }
    }

    public class ActualizarRolUsuarioRequestModel
    {
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
        public int? ProfesionalMedicoId { get; set; }
    }

    public class AdminActualizarEstadoCitaRequestModel
    {
        public int EstadoCita { get; set; }
    }

    public class AdminEditarUsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Rol")]
        public int RolId { get; set; }

        [Display(Name = "Doctor asociado")]
        public int? ProfesionalMedicoId { get; set; }

        public List<AdminDoctorModel> Doctores { get; set; } = new();
    }
}
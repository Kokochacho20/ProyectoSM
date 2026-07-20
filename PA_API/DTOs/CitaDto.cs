using PA_API.Enums;
using System.ComponentModel.DataAnnotations;

namespace PA_API.DTOs
{
    public class CrearCitaRequestDto
    {
        [Required]
        public int UsuarioId { get; set; }
        [Required]
        public int ProfesionalMedicoId { get; set; }
        [Required]
        public DateTime FechaHoraInicio { get; set; }
        [Required]
        public bool EsParaOtraPersona { get; set; }
        [Required]
        public string NombrePaciente { get; set; } = string.Empty;
        [Required]
        public string IdentificacionPaciente { get; set; } = string.Empty;
        [Required]
        public DateTime FechaNacimientoPaciente { get; set; }
        [Required]
        public string CorreoPaciente { get; set; } = string.Empty;
        public string TelefonoPaciente { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }

    public class CitaResponseDto
    {
        public int Id { get; set; }
        public int ProfesionalMedicoId { get; set; }
        public string ProfesionalMedico { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public EstadoCita EstadoCita { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string IdentificacionPaciente { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }
}
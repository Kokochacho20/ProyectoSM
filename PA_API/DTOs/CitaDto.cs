using PA_API.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        public int UsuarioId { get; set; }
        public int ProfesionalMedicoId { get; set; }
        public string ProfesionalMedico { get; set; } = string.Empty;

        // Informacion de paciente
        public string NombrePaciente { get; set; } = string.Empty;
        public string IdentificacionPaciente { get; set; } = string.Empty;
        public DateTime FechaNacimientoPaciente { get; set; }
        public string CorreoPaciente { get; set; } = string.Empty;
        public string TelefonoPaciente { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;

        public EstadoCita EstadoCita { get; set; }
        public DateTime FechaHoraInicio { get; set; }
    }

    public class ModificarCitaRequestDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime FechaHoraInicio { get; set; }
    }

    public class CancelarCitaRequestDto
    {
        [Required]
        public int UsuarioId { get; set; }
    }

    public class CitaDto
    {
        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("profesionalMedicoId")]
        public int ProfesionalMedicoId { get; set; }

        [JsonPropertyName("fechaHoraInicio")]
        public DateTime FechaHoraInicio { get; set; }

        [JsonPropertyName("esParaOtraPersona")]
        public bool EsParaOtraPersona { get; set; }

        [JsonPropertyName("nombrePaciente")]
        public string? NombrePaciente { get; set; }

        [JsonPropertyName("identificacionPaciente")]
        public string? IdentificacionPaciente { get; set; }

        [JsonPropertyName("fechaNacimientoPaciente")]
        public DateTime? FechaNacimientoPaciente { get; set; }

        [JsonPropertyName("correoPaciente")]
        public string? CorreoPaciente { get; set; }

        [JsonPropertyName("telefonoPaciente")]
        public string? TelefonoPaciente { get; set; }

        [JsonPropertyName("motivo")]
        public string? Motivo { get; set; }
    }

    public class DisponibilidadSlotDto
    {
        public DateTime Fecha { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Disponible { get; set; }
    }
}
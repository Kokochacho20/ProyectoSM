using System.ComponentModel.DataAnnotations;

namespace PA_WEB.Models
{
    public class CrearCitaModel
    {
        public int ProfesionalMedicoId { get; set; }
        public string? ProfesionalMedicoNombre { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe indicar la hora")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        public bool EsParaOtraPersona { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la identificación del paciente")]
        public string IdentificacionPaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimientoPaciente { get; set; }

        [Required(ErrorMessage = "Ingrese el correo del paciente")]
        [EmailAddress]
        public string CorreoPaciente { get; set; } = string.Empty;

        public string TelefonoPaciente { get; set; } = string.Empty;

        public string? Motivo { get; set; }
    }
}
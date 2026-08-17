using System.ComponentModel.DataAnnotations;

namespace PA_WEB.Models
{
    public class ModificarCitaModel
    {
        [Display(Name = "Nombre del paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        [Display(Name = "Identificación")]
        public string IdentificacionPaciente { get; set; } = string.Empty;

        [Display(Name = "Motivo / notas")]
        [Required(ErrorMessage = "El campo motivo es obligatorio.")]
        [MinLength(10, ErrorMessage = "El motivo debe tener al menos 10 caracteres.")]
        public string Motivo { get; set; } = string.Empty;

        public int ProfesionalId { get; set; }
        public int CitaId { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe indicar la hora")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        // Datos originales de la cita
        public DateTime FechaOriginal { get; set; }
        public TimeSpan HoraOriginal { get; set; }
    }
}
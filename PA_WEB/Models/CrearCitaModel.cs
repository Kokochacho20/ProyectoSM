using System.ComponentModel.DataAnnotations;

namespace PA_WEB.Models
{
    public class CrearCitaModel : IValidatableObject
    {
        public int ProfesionalMedicoId { get; set; }

        public string ProfesionalMedicoNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de la cita es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La hora de la cita es obligatoria")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora")]
        public TimeSpan Hora { get; set; } = new TimeSpan(8, 0, 0);

        [Display(Name = "Es para otra persona")]
        public bool EsParaOtraPersona { get; set; }

        [Required(ErrorMessage = "El nombre del paciente es obligatorio")]
        [Display(Name = "Nombre del paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación del paciente es obligatoria")]
        [Display(Name = "Identificación")]
        public string IdentificacionPaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento del paciente es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime FechaNacimientoPaciente { get; set; }

        [Required(ErrorMessage = "El correo del paciente es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido")]
        [Display(Name = "Correo")]
        public string CorreoPaciente { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string TelefonoPaciente { get; set; } = string.Empty;

        [Display(Name = "Motivo / notas")]
        public string Motivo { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Fecha == DateTime.MinValue || Fecha.Year < 1753)
            {
                yield return new ValidationResult(
                    "Debe seleccionar una fecha válida para la cita.",
                    new[] { nameof(Fecha) });
            }

            if (Fecha.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "La fecha de la cita no puede ser anterior al día actual.",
                    new[] { nameof(Fecha) });
            }

            if (FechaNacimientoPaciente == DateTime.MinValue || FechaNacimientoPaciente.Year < 1753)
            {
                yield return new ValidationResult(
                    "Debe ingresar una fecha de nacimiento válida.",
                    new[] { nameof(FechaNacimientoPaciente) });
            }
        }
    }
}
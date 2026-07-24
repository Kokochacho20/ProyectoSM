namespace PA_WEB.Models
{
    public class CitaModel
    {
        public int Id { get; set; }
        public int ProfesionalMedicoId { get; set; }
        public string ProfesionalMedico { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public EstadoCitaModel EstadoCita { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string IdentificacionPaciente { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }
}
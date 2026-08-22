namespace PA_API.DTOs
{
    public class MedicoDashboardDto
    {
        public int CitasPendientesAprobar { get; set; }
        public int CitasAprobadas { get; set; }
        public int CitasCanceladas { get; set; }
        public int CitasFinalizadas { get; set; }
        public int CitasHoy { get; set; }
        public int NotificacionesPendientes { get; set; }
    }

    public class MedicoCitaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int ProfesionalMedicoId { get; set; }
        public string ProfesionalMedico { get; set; } = string.Empty;

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

    public class MedicoActualizarEstadoCitaRequestDto
    {
        public int EstadoCita { get; set; }
    }
}
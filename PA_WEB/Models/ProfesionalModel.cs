namespace PA_WEB.Models
{
    public class ProfesionalModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CodigoMedico { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioConsulta { get; set; }
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Especialidades { get; set; } = string.Empty;
    }

    public class DisponibilidadSlotDto
    {
        public DateTime Fecha { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Disponible { get; set; }
    }

    public class CalendarioDiaDto
    {
        public DateTime Fecha { get; set; }
        public bool TieneDisponibilidad { get; set; }
        public bool EsPasado { get; set; }
    }

    public class CalendarioViewModel
    {
        public int ProfesionalId { get; set; }
        public string ProfesionalNombre { get; set; } = string.Empty;
        public DateTime MesActual { get; set; }
        public List<CalendarioDiaDto> Dias { get; set; } = new();
        public List<DisponibilidadSlotDto> Slots { get; set; } = new();
        public int? CitaId { get; set; }

        public DateTime? FechaCitaOriginal { get; set; }
        public TimeSpan? HoraCitaOriginal { get; set; }

        public DateTime? FechaSeleccionada { get; set; }
        public TimeSpan? HoraSeleccionada { get; set; }
    }
}
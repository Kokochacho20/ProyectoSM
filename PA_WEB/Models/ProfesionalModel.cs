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
}
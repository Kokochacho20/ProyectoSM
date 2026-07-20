namespace PA_API.DTOs
{
    public class EspecialidadDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class BuscarProfesionalesQueryDto
    {
        public string? Texto { get; set; }   
        public int? EspecialidadId { get; set; } 
    }

    public class ProfesionalMedicoDto
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
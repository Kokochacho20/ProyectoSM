namespace PA_WEB.Models
{
    public class NotificacionModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? CitaMedicaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
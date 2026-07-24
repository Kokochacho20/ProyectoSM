using System.Text.Json.Serialization;

namespace PA_WEB.Models
{
    public class EspecialidadModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("estado")]
        public bool Estado { get; set; }
    }
}
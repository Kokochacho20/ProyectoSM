using System.ComponentModel.DataAnnotations;

namespace PA_WEB.Models
{
    public class ModificarCitaModel
    {
        public int CitaId { get; set; }

        [Required(ErrorMessage = "Debe indicar la fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe indicar la hora")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace PA_WEB.Models
{
    public class RecuperarAccesoModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string CorreoElectronico { get; set; } = string.Empty;
    }
}
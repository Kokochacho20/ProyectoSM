namespace PA_WEB.Models
{
    public class InicioSesionResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public UsuarioModel Usuario { get; set; } = default!;
    }
}
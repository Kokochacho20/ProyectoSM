using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PA_WEB.Filters
{
    public class RequiereSesion : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");
            var token = context.HttpContext.Session.GetString("Token");

            if (usuarioId is null || string.IsNullOrWhiteSpace(token))
            {
                context.HttpContext.Session.Clear();

                context.Result = new RedirectToActionResult(
                    "Index",
                    "Home",
                    new { mensaje = "Debe iniciar sesión para acceder al sistema." });

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
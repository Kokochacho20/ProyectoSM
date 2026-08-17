using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PA_WEB.Filters
{
    public class ApiAuthorizationFilter(
        ITempDataDictionaryFactory tempDataFactory) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var executedContext = await next();

            var httpContext = executedContext.HttpContext;

            if (httpContext.Items.ContainsKey("ApiUnauthorized"))
            {
                httpContext.Session.Clear();

                var tempData = tempDataFactory.GetTempData(httpContext);

                tempData["Mensaje"] =
                    "La sesión expiró o el token no es válido. " +
                    "Inicie sesión nuevamente.";

                executedContext.Result =
                    new RedirectToActionResult(
                        "Index",
                        "Home",
                        null);

                return;
            }

            if (httpContext.Items.ContainsKey("ApiForbidden"))
            {
                executedContext.Result =
                    new RedirectToActionResult(
                        "Index",  // acceso denegado - fuera del scope actual.
                        "Home",
                        null);
            }
        }
    }
}
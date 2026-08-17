using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace PA_WEB.Filters
{
    public class ApiAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var token = httpContext?.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (request.RequestUri?.AbsoluteUri.Contains("IniciarSesion") == true) 
                return response;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                httpContext?.Items.TryAdd("ApiUnauthorized", true);

                response.Content = new StringContent(
                    "null",
                    Encoding.UTF8,
                    "application/json");
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                httpContext?.Items.TryAdd("ApiForbidden", true);

                response.Content = new StringContent(
                    "null",
                    Encoding.UTF8,
                    "application/json");
            }

            return response;
        }
    }
}
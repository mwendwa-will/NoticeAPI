using System.Net;

namespace NoticeAPI.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeader = "X-Api-Key";
        private readonly string _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _apiKey = config["ApiKey"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var isProtected = endpoint?.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Any(m =>
                m == "POST" || m == "PUT" || m == "DELETE") ?? false;

            if (isProtected && (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var key) || key != _apiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid or missing API key");
                return;
            }
            await _next(context);
        }
    }

    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKey(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}

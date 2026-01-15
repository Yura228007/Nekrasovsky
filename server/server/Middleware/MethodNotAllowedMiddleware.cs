using Microsoft.AspNetCore.Http;

namespace server.Middleware
{
    /// <summary>
    /// Middleware для обработки 405 Method Not Allowed ошибок
    /// </summary>
    public class MethodNotAllowedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MethodNotAllowedMiddleware> _logger;

        public MethodNotAllowedMiddleware(RequestDelegate next, ILogger<MethodNotAllowedMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Если статус код 404 и это не GET запрос, возможно это 405
            if (context.Response.StatusCode == 404 && 
                context.Request.Method != "GET" && 
                !context.Request.Path.Value?.Contains("swagger") == true)
            {
                // Проверяем, может ли быть это 405 ошибка
                var routeData = context.GetRouteData();
                if (routeData?.Values.Count > 0)
                {
                    // Если есть данные маршрута, но вернулся 404, возможно это 405
                    _logger.LogWarning("Possible 405 Method Not Allowed for {Method} {Path}", 
                        context.Request.Method, context.Request.Path);
                }
            }
        }
    }
}

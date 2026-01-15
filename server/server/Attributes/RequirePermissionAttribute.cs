using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using server.Services;
using System;

namespace server.Attributes
{
    /// <summary>
    /// Атрибут для проверки прав доступа пользователя к действию контроллера
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _permissionCode;

        public RequirePermissionAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Получаем userId из заголовка или query параметра
            var userId = GetUserIdFromRequest(context.HttpContext);

            if (!userId.HasValue)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "User ID is required. Please provide 'X-User-Id' header or 'userId' query parameter." });
                return;
            }

            // Получаем сервис для проверки прав
            var userPermissionsService = context.HttpContext.RequestServices.GetRequiredService<IUserPermissionsService>();

            // Проверяем права пользователя
            var hasPermission = await userPermissionsService.HasPermissionAsync(userId.Value, _permissionCode);

            // Также проверяем, является ли пользователь админом (логин "admin" или право "Admin")
            if (!hasPermission)
            {
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                var user = await userService.GetUserByIdAsync(userId.Value);
                
                if (user != null && user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    hasPermission = true;
                }
                else
                {
                    // Проверяем право Admin
                    hasPermission = await userPermissionsService.HasPermissionAsync(userId.Value, "Admin");
                }
            }

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                context.HttpContext.Response.StatusCode = 403;
                await context.HttpContext.Response.WriteAsJsonAsync(new { message = $"Access denied. Required permission: {_permissionCode}" });
                return;
            }

            await next();
        }

        private int? GetUserIdFromRequest(HttpContext context)
        {
            // Пытаемся получить из заголовка
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                if (int.TryParse(userIdHeader.ToString(), out var userId))
                {
                    return userId;
                }
            }

            // Пытаемся получить из query параметра
            if (context.Request.Query.TryGetValue("userId", out var userIdQuery))
            {
                if (int.TryParse(userIdQuery.ToString(), out var userId))
                {
                    return userId;
                }
            }

            return null;
        }
    }
}

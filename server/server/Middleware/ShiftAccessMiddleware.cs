using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Services;
using System.Text.Json;

namespace server.Middleware
{
    public class ShiftAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ShiftAccessMiddleware> _logger;

        public ShiftAccessMiddleware(RequestDelegate next, ILogger<ShiftAccessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AppDbContext dbContext,
            IUserPermissionsService userPermissionsService,
            IRoleService roleService,
            IUserService userService)
        {
            if (!IsApiRequest(context) || IsAuthRequest(context) || IsSafeMethod(context))
            {
                await _next(context);
                return;
            }

            var userId = GetUserIdFromRequest(context);
            if (!userId.HasValue)
            {
                await WriteJsonErrorAsync(context, StatusCodes.Status401Unauthorized,
                    "User ID is required. Please provide 'X-User-Id' header or 'userId' query parameter.");
                return;
            }

            var isPrivilegedUser = await IsPrivilegedUserAsync(userId.Value, userService, roleService);
            var hasShiftTransferPermission = await HasPermissionAsync(
                userId.Value,
                "ShiftTransfer",
                userPermissionsService,
                roleService,
                userService);

            if (IsWorkReportStartRequest(context))
            {
                if (hasShiftTransferPermission && !isPrivilegedUser)
                {
                    await WriteJsonErrorAsync(context, StatusCodes.Status403Forbidden,
                        "Users with ShiftTransfer permission cannot start shifts manually.");
                    return;
                }

                await _next(context);
                return;
            }

            if (IsWorkReportFinishRequest(context) && hasShiftTransferPermission && !isPrivilegedUser)
            {
                await WriteJsonErrorAsync(context, StatusCodes.Status403Forbidden,
                    "Users with ShiftTransfer permission cannot finish shifts manually.");
                return;
            }

            if (IsAlarmRequest(context) || IsShiftTransferConfirmRequest(context))
            {
                await _next(context);
                return;
            }

            if (IsShiftTransferCreateRequest(context))
            {
                var hasActiveShiftForTransfer = await dbContext.WorkReports
                    .AnyAsync(wr => wr.UserId == userId.Value && wr.FinishWork == null);

                if (!hasActiveShiftForTransfer)
                {
                    await WriteJsonErrorAsync(context, StatusCodes.Status403Forbidden,
                        "Shift is not started. You cannot transfer a shift without an active shift.");
                    return;
                }

                await _next(context);
                return;
            }

            var hasActiveShift = await dbContext.WorkReports
                .AnyAsync(wr => wr.UserId == userId.Value && wr.FinishWork == null);

            if (!hasActiveShift)
            {
                await WriteJsonErrorAsync(context, StatusCodes.Status403Forbidden,
                    "Shift is not started. Only shift acceptance is allowed.");
                return;
            }

            await _next(context);
        }

        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api");
        }

        private static bool IsAuthRequest(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            return path.StartsWith("/api/auth") || path.StartsWith("/api/users/authenticate");
        }

        private static bool IsSafeMethod(HttpContext context)
        {
            return HttpMethods.IsGet(context.Request.Method) ||
                   HttpMethods.IsHead(context.Request.Method) ||
                   HttpMethods.IsOptions(context.Request.Method);
        }

        private static bool IsWorkReportStartRequest(HttpContext context)
        {
            return HttpMethods.IsPost(context.Request.Method) &&
                   context.Request.Path.Value?.Equals("/api/workreports/start", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool IsWorkReportFinishRequest(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            return HttpMethods.IsPost(context.Request.Method) &&
                   path.StartsWith("/api/workreports/") &&
                   path.EndsWith("/finish");
        }

        private static bool IsShiftTransferConfirmRequest(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            return HttpMethods.IsPost(context.Request.Method) &&
                   path.StartsWith("/api/shifttransfers/") &&
                   path.EndsWith("/confirm");
        }

        private static bool IsShiftTransferCreateRequest(HttpContext context)
        {
            return HttpMethods.IsPost(context.Request.Method) &&
                   context.Request.Path.Value?.Equals("/api/shifttransfers", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool IsAlarmRequest(HttpContext context)
        {
            return HttpMethods.IsPost(context.Request.Method) &&
                   context.Request.Path.Value?.Equals("/api/alarmevents", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static int? GetUserIdFromRequest(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) &&
                int.TryParse(userIdHeader.ToString(), out var headerUserId))
            {
                return headerUserId;
            }

            if (context.Request.Query.TryGetValue("userId", out var userIdQuery) &&
                int.TryParse(userIdQuery.ToString(), out var queryUserId))
            {
                return queryUserId;
            }

            return null;
        }

        private static async Task<bool> HasPermissionAsync(
            int userId,
            string permissionCode,
            IUserPermissionsService userPermissionsService,
            IRoleService roleService,
            IUserService userService)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.RoleId.HasValue)
            {
                var rolePermissions = await roleService.GetRolePermissionsAsync(user.RoleId.Value);
                if (rolePermissions.Any(p => p.Code == permissionCode))
                {
                    return true;
                }
            }

            if (await userPermissionsService.HasPermissionAsync(userId, permissionCode))
            {
                return true;
            }

            if (user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (user.RoleId.HasValue)
            {
                var role = await roleService.GetRoleByIdAsync(user.RoleId.Value);
                if (role != null &&
                    (role.Code == "Owner" || role.Code == "Admin" ||
                     role.Name == "Владелец" || role.Name == "Администратор"))
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task<bool> IsPrivilegedUserAsync(int userId, IUserService userService, IRoleService roleService)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (user.RoleId.HasValue)
            {
                var role = await roleService.GetRoleByIdAsync(user.RoleId.Value);
                if (role != null &&
                    (role.Code == "Owner" || role.Code == "Admin" ||
                     role.Name == "Владелец" || role.Name == "Администратор"))
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task WriteJsonErrorAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new { message });
            await context.Response.WriteAsync(payload);
        }
    }
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace server.Extensions;

/// <summary>
/// Добавляет в Swagger UI параметр заголовка X-User-Id для тестирования API от имени пользователя.
/// </summary>
public class SwaggerXUserIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        if (operation.Parameters.Any(p => p.Name.Equals("X-User-Id", StringComparison.OrdinalIgnoreCase)))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-Id",
            In = ParameterLocation.Header,
            Description = "ID текущего пользователя (обязателен для endpoints, проверяющих права: смена ответственного и т.д.). Укажите ID пользователя с ролью Owner/Admin или с разрешением ManageResponsibility.",
            Required = false,
            Schema = new OpenApiSchema { Type = "integer", Format = "int32", Example = new Microsoft.OpenApi.Any.OpenApiInteger(1) }
        });
    }
}

using server.Services;

namespace server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register password service
            services.AddScoped<IPasswordService, PasswordService>();

            // Register user service
            services.AddScoped<IUserService, UserService>();

            // Register permission service
            services.AddScoped<IPermissionService, PermissionService>();

            // Register user permissions service
            services.AddScoped<IUserPermissionsService, UserPermissionsService>();

            // Register warehouse service
            services.AddScoped<IWarehouseService, WarehouseService>();

            // Register material service
            services.AddScoped<IMaterialService, MaterialService>();

            // Register product service
            services.AddScoped<IProductService, ProductService>();

            // Register recipe service
            services.AddScoped<IRecipeService, RecipeService>();

            // Register accessible movement service
            services.AddScoped<IAccessibleMovementService, AccessibleMovementService>();

            // Register part request service
            services.AddScoped<IPartRequestService, PartRequestService>();

            // Register alarm event service
            services.AddScoped<IAlarmEventService, AlarmEventService>();

            // Register shift transfer service
            services.AddScoped<IShiftTransferService, ShiftTransferService>();

            // Register filling warehouse service
            services.AddScoped<IFillingWarehouseService, FillingWarehouseService>();

            // Register work report service
            services.AddScoped<IWorkReportService, WorkReportService>();

            // Register request log service
            services.AddScoped<IRequestLogService, RequestLogService>();

            // Register backup service
            services.AddScoped<IBackupService, BackupService>();

            // Register backup background service
            services.AddHostedService<BackupBackgroundService>();

            return services;
        }
    }
}


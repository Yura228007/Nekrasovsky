using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Services
{
    public interface IApiService
    {
        // Users
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> SearchUsersAsync(string? name, string? surname);
        Task<ApiResponse<User>> AddUserAsync(User user);
        Task<ApiResponse<User>> EditUserAsync(int id, User user);
        Task<ApiResponse<object>> DeleteUserAsync(int id);

        // Products
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> SearchProductsAsync(string? name, string? code);
        Task<ApiResponse<Product>> AddProductAsync(Product product);
        Task<ApiResponse<Product>> EditProductAsync(int id, Product product);
        Task<ApiResponse<object>> DeleteProductAsync(int id);

        // Materials
        Task<List<Material>> GetAllMaterialsAsync();
        Task<Material?> GetMaterialByIdAsync(int id);
        Task<List<Material>> SearchMaterialsAsync(string? name, string? code);
        Task<ApiResponse<Material>> AddMaterialAsync(Material material);
        Task<ApiResponse<Material>> EditMaterialAsync(int id, Material material);
        Task<ApiResponse<object>> DeleteMaterialAsync(int id);

        // Warehouses
        Task<List<Warehouse>> GetAllWarehousesAsync();
        Task<Warehouse?> GetWarehouseByIdAsync(int id);
        Task<List<Warehouse>> SearchWarehousesAsync(string? name, string? type);
        Task<ApiResponse<Warehouse>> AddWarehouseAsync(Warehouse warehouse);
        Task<ApiResponse<Warehouse>> EditWarehouseAsync(int id, Warehouse warehouse);
        Task<ApiResponse<object>> DeleteWarehouseAsync(int id);
        Task<ApiResponse<Warehouse>> StopWarehouseAsync(int id);
        Task<ApiResponse<Warehouse>> StartWarehouseAsync(int id);

        // Work Reports
        Task<List<WorkReport>> GetAllWorkReportsAsync();
        Task<WorkReport?> GetWorkReportByIdAsync(int id);
        Task<List<WorkReport>> GetWorkReportsByUserAsync(int userId);
        Task<List<WorkReport>> GetActiveWorkReportsAsync(int userId);
        Task<ApiResponse<WorkReport>> AddWorkReportAsync(WorkReport report);
        Task<ApiResponse<WorkReport>> StartWorkAsync(StartWorkRequest request);
        Task<ApiResponse<WorkReport>> FinishWorkAsync(int id, FinishWorkRequest? request);

        // Part Requests
        Task<List<PartRequest>> GetAllPartRequestsAsync();
        Task<PartRequest?> GetPartRequestByIdAsync(int id);
        Task<ApiResponse<PartRequest>> AddPartRequestAsync(PartRequest request);
        Task<ApiResponse<PartRequest>> ApprovePartRequestAsync(int id);
        Task<ApiResponse<PartRequest>> RejectPartRequestAsync(int id, string? reason);

        // Alarm Events
        Task<List<AlarmEvent>> GetAllAlarmEventsAsync();
        Task<List<AlarmEvent>> GetAlarmEventsByUserAsync(int userId);
        Task<ApiResponse<AlarmEvent>> AddAlarmEventAsync(AlarmEvent alarmEvent);

        // User Permissions
        Task<List<Permission>> GetUserPermissionsAsync(int userId);
        Task<bool> CheckPermissionAsync(int userId, string permissionCode);

        // Roles
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);

        // Database
        Task<Dictionary<string, object>> CheckDatabaseAsync();
    }

    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? User { get; set; }
        public T? Product { get; set; }
        public T? Material { get; set; }
        public T? Warehouse { get; set; }
        public T? Report { get; set; }
        public T? Request { get; set; }
        public T? AlarmEvent { get; set; }
        public T? Role { get; set; }

        public T? GetData()
        {
            return User ?? Product ?? Material ?? Warehouse ?? Report ?? Request ?? AlarmEvent ?? Role;
        }
    }

    public class StartWorkRequest
    {
        public int UserId { get; set; }
        public string? StartTime { get; set; }
    }

    public class FinishWorkRequest
    {
        public string? FinishTime { get; set; }
    }
}


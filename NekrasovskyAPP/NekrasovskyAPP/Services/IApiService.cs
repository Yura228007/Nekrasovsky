using System;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Services
{
    public interface IApiService
    {
        // Users
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> SearchUsersAsync(string? name, string? surname);
        Task<ApiResponse<User>> AuthenticateAsync(string login, string password);
        Task<ApiResponse<User>> AddUserAsync(User user);
        Task<ApiResponse<User>> EditUserAsync(int id, User user);
        Task<ApiResponse<object>> DeleteUserAsync(int id);
        /// <summary>Отправить команду разблокировки/закрытия приложения на устройство пользователя (для админа).</summary>
        Task<ApiResponse<object>> UnlockUserDeviceAsync(int userId);

        // Products
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> SearchProductsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null);
        Task<ApiResponse<Product>> AddProductAsync(Product product);
        Task<ApiResponse<Product>> EditProductAsync(int id, Product product);
        Task<ApiResponse<object>> DeleteProductAsync(int id);

        // Materials
        Task<List<Material>> GetAllMaterialsAsync();
        Task<Material?> GetMaterialByIdAsync(int id);
        Task<List<Material>> SearchMaterialsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null);
        Task<ApiResponse<Material>> AddMaterialAsync(Material material, int? quantity = null, string? measuringUnit = null, int? warehouseId = null);
        Task<ApiResponse<Material>> EditMaterialAsync(int id, Material material);
        Task<ApiResponse<object>> DeleteMaterialAsync(int id);

        // Warehouses
        Task<List<Warehouse>> GetAllWarehousesAsync();
        Task<Warehouse?> GetWarehouseByIdAsync(int id);
        Task<List<Warehouse>> SearchWarehousesAsync(string? name, string? type, bool? isActive = null, string? sortBy = null);
        Task<ApiResponse<Warehouse>> AddWarehouseAsync(Warehouse warehouse);
        Task<ApiResponse<Warehouse>> EditWarehouseAsync(int id, Warehouse warehouse);
        Task<ApiResponse<object>> DeleteWarehouseAsync(int id);
        Task<ApiResponse<Warehouse>> StopWarehouseAsync(int id);
        Task<ApiResponse<Warehouse>> StartWarehouseAsync(int id);

        // Filling Warehouses
        Task<List<FillingWarehouse>> GetAllFillingWarehousesAsync();
        Task<FillingWarehouse?> GetFillingByMaterialAsync(int warehouseId, int materialId);
        Task<FillingWarehouse?> GetFillingByProductAsync(int warehouseId, int productId);
        Task<List<FillingWarehouse>> GetFillingsByWarehouseAsync(int warehouseId);
        Task<List<FillingWarehouse>> GetFillingsByMaterialAsync(int materialId);
        Task<List<FillingWarehouse>> GetFillingsByProductAsync(int productId);
        Task<ApiResponse<FillingWarehouse>> AddFillingWarehouseAsync(FillingWarehouse filling);
        Task<ApiResponse<FillingWarehouse>> EditFillingWarehouseByMaterialAsync(int warehouseId, int materialId, FillingWarehouse filling);
        Task<ApiResponse<FillingWarehouse>> EditFillingWarehouseByProductAsync(int warehouseId, int productId, FillingWarehouse filling);
        Task<ApiResponse<object>> DeleteFillingWarehouseByMaterialAsync(int warehouseId, int materialId);
        Task<ApiResponse<object>> DeleteFillingWarehouseByProductAsync(int warehouseId, int productId);
        Task<ApiResponse<FillingWarehouse>> UpdateFillingQuantityByMaterialAsync(FillingWarehouse filling);
        Task<ApiResponse<FillingWarehouse>> UpdateFillingQuantityByProductAsync(FillingWarehouse filling);

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
        Task<List<PartRequest>> GetPartRequestsByUserAsync(int userId, bool sent = true);
        Task<ApiResponse<PartRequest>> AddPartRequestAsync(PartRequest request);
        Task<ApiResponse<PartRequest>> ApprovePartRequestAsync(int id);
        Task<ApiResponse<PartRequest>> RejectPartRequestAsync(int id, string? reason);
        Task<ApiResponse<object>> CancelPartRequestAsync(int id);

        // Shift Transfers
        Task<List<ShiftTransfer>> GetShiftTransfersByUserAsync(int userId, bool sent = true);
        Task<List<ShiftTransfer>> GetPendingShiftTransfersAsync(int userId);
        Task<ApiResponse<ShiftTransfer>> CreateShiftTransferAsync(ShiftTransfer transfer);
        Task<ApiResponse<ShiftTransfer>> ConfirmShiftTransferAsync(int id);
        Task<ApiResponse<object>> CancelShiftTransferAsync(int id);

        // Alarm Events
        Task<List<AlarmEvent>> GetAllAlarmEventsAsync();
        Task<List<AlarmEvent>> GetAlarmEventsByUserAsync(int userId);
        Task<ApiResponse<AlarmEvent>> AddAlarmEventAsync(AlarmEvent alarmEvent);

        // User Permissions
        Task<List<Permission>> GetUserPermissionsAsync(int userId);
        Task<bool> CheckPermissionAsync(int userId, string permissionCode);

        // Responsibilities
        Task<List<Responsibility>> GetResponsibilitiesByUserAsync(int userId, bool activeOnly = true);
        Task<Responsibility?> GetResponsibilityByMaterialAsync(int materialId, bool activeOnly = true);
        Task<Responsibility?> GetResponsibilityByProductAsync(int productId, bool activeOnly = true);
        Task<List<ResponsibilityStockItem>> GetResponsibilityStockAsync(int userId);
        Task<List<ResponsibilityAssignment>> GetActiveMaterialAssignmentsAsync();
        Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsAsync();

        // Product Batches
        Task<List<ProductBatch>> GetAllProductBatchesAsync();
        Task<ProductBatch?> GetProductBatchByIdAsync(int id);
        Task<List<ProductBatch>> GetProductBatchesByProductAsync(int productId);
        Task<List<ProductBatch>> GetProductBatchesByWarehouseAsync(int warehouseId);
        Task<List<ProductBatch>> GetProductBatchesByUserAsync(int userId);
        Task<ApiResponse<ProductBatch>> CreateProductBatchAsync(ProductBatch batch);
        Task<ApiResponse<ProductBatch>> UpdateProductBatchAsync(int id, ProductBatch batch);
        Task<ApiResponse<object>> DeleteProductBatchAsync(int id);

        // Product Movement Requests
        Task<List<ProductMovementRequest>> GetAllProductMovementRequestsAsync();
        Task<ProductMovementRequest?> GetProductMovementRequestByIdAsync(int id);
        Task<List<ProductMovementRequest>> GetProductMovementRequestsByStatusAsync(ProductMovementStatus status);
        Task<List<ProductMovementRequest>> GetSentProductMovementRequestsAsync(int userId);
        Task<List<ProductMovementRequest>> GetReceivedProductMovementRequestsAsync(int userId);
        Task<ApiResponse<ProductMovementRequest>> CreateProductMovementRequestAsync(ProductMovementRequest request);
        Task<ApiResponse<ProductMovementRequest>> UpdateProductMovementRequestAsync(int id, ProductMovementRequest request);
        Task<ApiResponse<ProductMovementRequest>> ApproveProductMovementRequestAsync(int id);
        Task<ApiResponse<ProductMovementRequest>> RejectProductMovementRequestAsync(int id, string? reason);
        Task<ApiResponse<object>> DeleteProductMovementRequestAsync(int id);
        Task<ApiResponse<Responsibility>> AssignMaterialResponsibilityAsync(int materialId, int userId, int? quantity = null, string? measuringUnit = null);
        Task<ApiResponse<Responsibility>> AssignProductResponsibilityAsync(int productId, int userId, int? quantity = null, string? measuringUnit = null);
        Task<ApiResponse<object>> TransferMaterialResponsibilityFillingAsync(int warehouseId, int materialId, int fromUserId, int toUserId, int? quantityToTransfer = null);
        Task<ApiResponse<object>> TransferProductResponsibilityFillingAsync(int warehouseId, int productId, int fromUserId, int toUserId, int? quantityToTransfer = null);
        Task<ApiResponse<object>> TransferBatchResponsibilityFillingAsync(int batchId, int fromUserId, int toUserId, int? quantityToTransfer = null);
        Task<ApiResponse<object>> ReleaseMaterialResponsibilityAsync(int materialId);
        Task<ApiResponse<object>> ReleaseProductResponsibilityAsync(int productId);
        Task<ApiResponse<object>> ReleaseBatchResponsibilityAsync(int batchId, int userId);

        // Reprocessing
        Task<ApiResponse<Reprocessing>> CreateReprocessingAsync(ReprocessingCreateRequest request);

        // Disposal (Утиль)
        Task<ApiResponse<object>> ProcessDisposalAsync(DisposalProcessRequest request);

        // Product Outputs
        Task<ProductOutputOptionsResponse?> GetProductOutputOptionsAsync();
        Task<List<ProductOutput>> GetAllProductOutputsAsync();
        Task<List<ProductOutput>> GetProductOutputsByUserAsync(int userId);
        Task<ProductOutput?> GetProductOutputByIdAsync(int id);
        Task<ApiResponse<ProductOutput>> AddProductOutputAsync(ProductOutput output);
        Task<ApiResponse<ProductOutput>> EditProductOutputAsync(int id, ProductOutput output);
        Task<ApiResponse<object>> DeleteProductOutputAsync(int id);

        // Machines
        Task<List<Machine>> GetAllMachinesAsync();
        Task<List<Machine>> GetActiveMachinesAsync();
        Task<Machine?> GetMachineByIdAsync(int id);
        Task<ApiResponse<Machine>> AddMachineAsync(Machine machine);
        Task<ApiResponse<Machine>> EditMachineAsync(int id, Machine machine);
        Task<ApiResponse<object>> DeleteMachineAsync(int id);

        // Roles
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);

        // Permissions
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<List<Permission>> GetRolePermissionsAsync(int roleId);
        Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);

        // Database
        Task<Dictionary<string, object>> CheckDatabaseAsync();

        // History
        Task<List<HistoryEvent>?> GetHistoryAsync(
            int? userId = null,
            int? relatedUserId = null,
            string? action = null,
            string? entityType = null,
            int? warehouseId = null,
            int? materialId = null,
            int? productId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // Shift Reports
        Task<List<ShiftReport>> GetShiftReportsByUserAsync(int userId);
        Task<ShiftReport?> GetShiftReportByWorkReportIdAsync(int workReportId);
        Task<byte[]?> DownloadShiftReportAsync(int reportId);
    }

    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? User { get; set; }
        public T? Product { get; set; }
        public T? Material { get; set; }
        public T? Filling { get; set; }
        public T? Warehouse { get; set; }
        public T? Report { get; set; }
        public T? Request { get; set; }
        public T? AlarmEvent { get; set; }
        public T? Role { get; set; }
        public T? Responsibility { get; set; }
        public T? Reprocessing { get; set; }
        public T? Transfer { get; set; }
        public T? Output { get; set; }
        public T? Machine { get; set; }
        public T? Batch { get; set; }

        public bool IsSuccess =>
            GetData() != null ||
            string.IsNullOrWhiteSpace(Message) ||
            Message.Contains("success", StringComparison.OrdinalIgnoreCase) ||
            Message.Contains("успеш", StringComparison.OrdinalIgnoreCase) ||
            Message.Contains("transferred", StringComparison.OrdinalIgnoreCase) ||
            Message.Contains("released", StringComparison.OrdinalIgnoreCase) ||
            Message.Contains("assigned", StringComparison.OrdinalIgnoreCase);

        public T? GetData()
        {
            return User ?? Product ?? Material ?? Filling ?? Warehouse ?? Report ?? Request ?? AlarmEvent ?? Role ?? Responsibility ?? Reprocessing ?? Transfer ?? Output ?? Machine ?? Batch;
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
        public string? Note { get; set; }
    }

}


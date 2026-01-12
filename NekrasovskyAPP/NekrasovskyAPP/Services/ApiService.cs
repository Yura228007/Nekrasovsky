using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        
        private static string GetBaseUrl()
        {
#if ANDROID
            // Для Android:
            // - Эмулятор: "http://10.0.2.2:5000/"
            // - Реальное устройство: используйте IP вашего компьютера в локальной сети
            //   Например: "http://192.168.1.100:5000/"
            //   Найдите IP через: ifconfig (Mac/Linux) или ipconfig (Windows)
            //   Важно: устройство и компьютер должны быть в одной Wi-Fi сети
            
            // Попробуем определить, эмулятор это или реальное устройство
            // Для реального устройства используйте IP вашего компьютера
            // Замените на ваш IP адрес для тестирования на реальном устройстве
            string androidUrl = "http://10.0.2.2:5000/"; // По умолчанию для эмулятора
            
            // Раскомментируйте и укажите IP вашего компьютера для реального устройства:
            androidUrl = "http://192.168.1.121:5000/"; // Замените XXX на ваш IP
            
            return androidUrl;
#else
            // Для Windows/Desktop/iOS/Mac
            return "http://localhost:5000/";
#endif
        }

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(GetBaseUrl());
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Users
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<User>>(_jsonOptions) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<User>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<User>> SearchUsersAsync(string? name, string? surname)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(surname)) queryParams.Add($"surname={Uri.EscapeDataString(surname)}");
                
                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/users/search{query}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<User>>(_jsonOptions) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<ApiResponse<User>> AddUserAsync(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users/add", user, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<User>>(_jsonOptions) ?? new ApiResponse<User>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<User> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<User>> EditUserAsync(int id, User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/users/edit/{id}", user, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<User>>(_jsonOptions) ?? new ApiResponse<User>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<User> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/users/delete/{id}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions) ?? new ApiResponse<object>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Products
        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions) ?? new List<Product>();
            }
            catch
            {
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string? name, string? code)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(code)) queryParams.Add($"code={Uri.EscapeDataString(code)}");
                
                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/products/search{query}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions) ?? new List<Product>();
            }
            catch
            {
                return new List<Product>();
            }
        }

        public async Task<ApiResponse<Product>> AddProductAsync(Product product)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products/add", product, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<Product>>(_jsonOptions) ?? new ApiResponse<Product>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Product> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Product>> EditProductAsync(int id, Product product)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/products/edit/{id}", product, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<Product>>(_jsonOptions) ?? new ApiResponse<Product>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Product> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/products/delete/{id}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions) ?? new ApiResponse<object>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Materials
        public async Task<List<Material>> GetAllMaterialsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/materials");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Material>>(_jsonOptions) ?? new List<Material>();
            }
            catch
            {
                return new List<Material>();
            }
        }

        public async Task<Material?> GetMaterialByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/materials/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Material>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Material>> SearchMaterialsAsync(string? name, string? code)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(code)) queryParams.Add($"code={Uri.EscapeDataString(code)}");
                
                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/materials/search{query}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Material>>(_jsonOptions) ?? new List<Material>();
            }
            catch
            {
                return new List<Material>();
            }
        }

        public async Task<ApiResponse<Material>> AddMaterialAsync(Material material)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/materials/add", material, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<Material>>(_jsonOptions) ?? new ApiResponse<Material>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Material> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Material>> EditMaterialAsync(int id, Material material)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/materials/edit/{id}", material, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<Material>>(_jsonOptions) ?? new ApiResponse<Material>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Material> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteMaterialAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/materials/delete/{id}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions) ?? new ApiResponse<object>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Warehouses
        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/warehouses");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Warehouse>>(_jsonOptions) ?? new List<Warehouse>();
            }
            catch
            {
                return new List<Warehouse>();
            }
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/warehouses/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Warehouse>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Warehouse>> SearchWarehousesAsync(string? name, string? type)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={Uri.EscapeDataString(type)}");
                
                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/warehouses/search{query}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Warehouse>>(_jsonOptions) ?? new List<Warehouse>();
            }
            catch
            {
                return new List<Warehouse>();
            }
        }

        public async Task<ApiResponse<Warehouse>> AddWarehouseAsync(Warehouse warehouse)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/warehouses/add", warehouse, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<Warehouse>>(_jsonOptions) ?? new ApiResponse<Warehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Warehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Warehouse>> EditWarehouseAsync(int id, Warehouse warehouse)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/warehouses/edit/{id}", warehouse, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<Warehouse>>(_jsonOptions) ?? new ApiResponse<Warehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Warehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteWarehouseAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/warehouses/delete/{id}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions) ?? new ApiResponse<object>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Work Reports
        public async Task<List<WorkReport>> GetAllWorkReportsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/work-reports");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<WorkReport>>(_jsonOptions) ?? new List<WorkReport>();
            }
            catch
            {
                return new List<WorkReport>();
            }
        }

        public async Task<WorkReport?> GetWorkReportByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/work-reports/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<WorkReport>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<WorkReport>> GetWorkReportsByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/work-reports/user/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<WorkReport>>(_jsonOptions) ?? new List<WorkReport>();
            }
            catch
            {
                return new List<WorkReport>();
            }
        }

        public async Task<List<WorkReport>> GetActiveWorkReportsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/work-reports/user/{userId}/active");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<WorkReport>>(_jsonOptions) ?? new List<WorkReport>();
            }
            catch
            {
                return new List<WorkReport>();
            }
        }

        public async Task<ApiResponse<WorkReport>> AddWorkReportAsync(WorkReport report)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/work-reports/add", report, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<WorkReport>>(_jsonOptions) ?? new ApiResponse<WorkReport>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<WorkReport> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<WorkReport>> StartWorkAsync(StartWorkRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/work-reports/start", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<WorkReport>>(_jsonOptions) ?? new ApiResponse<WorkReport>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<WorkReport> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<WorkReport>> FinishWorkAsync(int id, FinishWorkRequest? request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/work-reports/{id}/finish", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<WorkReport>>(_jsonOptions) ?? new ApiResponse<WorkReport>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<WorkReport> { Message = ex.Message };
            }
        }

        // Part Requests
        public async Task<List<PartRequest>> GetAllPartRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/part-requests");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<PartRequest>>(_jsonOptions) ?? new List<PartRequest>();
            }
            catch
            {
                return new List<PartRequest>();
            }
        }

        public async Task<PartRequest?> GetPartRequestByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/part-requests/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PartRequest>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<PartRequest>> AddPartRequestAsync(PartRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/part-requests/add", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<PartRequest>>(_jsonOptions) ?? new ApiResponse<PartRequest>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PartRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<PartRequest>> ApprovePartRequestAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/part-requests/{id}/approve", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<PartRequest>>(_jsonOptions) ?? new ApiResponse<PartRequest>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PartRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<PartRequest>> RejectPartRequestAsync(int id, string? reason)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/part-requests/{id}/reject", reason, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<PartRequest>>(_jsonOptions) ?? new ApiResponse<PartRequest>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PartRequest> { Message = ex.Message };
            }
        }

        // Alarm Events
        public async Task<List<AlarmEvent>> GetAllAlarmEventsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/alarm-events");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<AlarmEvent>>(_jsonOptions) ?? new List<AlarmEvent>();
            }
            catch
            {
                return new List<AlarmEvent>();
            }
        }

        public async Task<List<AlarmEvent>> GetAlarmEventsByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/alarm-events/user/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<AlarmEvent>>(_jsonOptions) ?? new List<AlarmEvent>();
            }
            catch
            {
                return new List<AlarmEvent>();
            }
        }

        public async Task<ApiResponse<AlarmEvent>> AddAlarmEventAsync(AlarmEvent alarmEvent)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/alarm-events/add", alarmEvent, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<AlarmEvent>>(_jsonOptions) ?? new ApiResponse<AlarmEvent>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<AlarmEvent> { Message = ex.Message };
            }
        }

        // User Permissions
        public async Task<List<Permission>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/user-permissions/user/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Permission>>(_jsonOptions) ?? new List<Permission>();
            }
            catch
            {
                return new List<Permission>();
            }
        }

        public async Task<bool> CheckPermissionAsync(int userId, string permissionCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/user-permissions/check?userId={userId}&permissionCode={Uri.EscapeDataString(permissionCode)}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>(_jsonOptions);
                return result?.GetValueOrDefault("hasPermission", false) ?? false;
            }
            catch
            {
                return false;
            }
        }

        // Database
        public async Task<Dictionary<string, object>> CheckDatabaseAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/database/check");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(_jsonOptions);
                return result ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object> { { "success", false }, { "message", "Ошибка подключения к серверу" } };
            }
        }
    }
}


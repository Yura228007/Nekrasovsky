using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using NekrasovskyAPP.Models;
using System.Threading.Tasks;

namespace NekrasovskyAPP.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private int? _currentUserId;
        
        private static string GetBaseUrl()
        {
#if ANDROID
            //using var stream = await FileSystem.OpenAppPackageFileAsync("server_ip.txt");
            //using var reader = new StreamReader(stream);
            //return (await reader.ReadToEndAsync()).Trim().ToString();
            return "http://192.168.1.128:9000/";
#else 
            return "http://localhost:9000/";
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

        /// <summary>
        /// Устанавливает ID текущего пользователя для добавления в заголовки запросов
        /// </summary>
        public void SetCurrentUserId(int? userId)
        {
            _currentUserId = userId;
            if (userId.HasValue)
            {
                _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId.Value.ToString());
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            }
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

        public async Task<ApiResponse<User>> AuthenticateAsync(string login, string password)
        {
            try
            {
                var payload = new { login, password };
                var response = await _httpClient.PostAsJsonAsync("api/Users/authenticate", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("user"))
                {
                    var userJson = System.Text.Json.JsonSerializer.Serialize(result["user"]);
                    var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson, _jsonOptions);
                    return new ApiResponse<User>
                    {
                        User = user,
                        Message = result.ContainsKey("message")
                            ? result["message"]?.ToString() ?? "Login successful"
                            : "Login successful"
                    };
                }
                return new ApiResponse<User> { Message = "Failed to parse response" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<User> { Message = ex.Message };
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
                var response = await _httpClient.PostAsJsonAsync("api/users", user, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("user"))
                {
                    var userJson = System.Text.Json.JsonSerializer.Serialize(result["user"]);
                    var createdUser = System.Text.Json.JsonSerializer.Deserialize<User>(userJson, _jsonOptions);
                    return new ApiResponse<User> { User = createdUser, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "User created successfully" : "User created successfully" };
                }
                return new ApiResponse<User> { Message = "Failed to parse response" };
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
                var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", user, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("user"))
                {
                    var userJson = System.Text.Json.JsonSerializer.Serialize(result["user"]);
                    var updatedUser = System.Text.Json.JsonSerializer.Deserialize<User>(userJson, _jsonOptions);
                    return new ApiResponse<User> { User = updatedUser, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "User updated successfully" : "User updated successfully" };
                }
                return new ApiResponse<User> { Message = "Failed to parse response" };
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
                var response = await _httpClient.DeleteAsync($"api/users/{id}");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "User deleted successfully" : "User deleted successfully" };
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

        public async Task<List<Product>> SearchProductsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(code)) queryParams.Add($"code={Uri.EscapeDataString(code)}");
                if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value.ToString().ToLower()}");
                if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

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
                var response = await _httpClient.PostAsJsonAsync("api/products", product, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("product"))
                {
                    var productJson = System.Text.Json.JsonSerializer.Serialize(result["product"]);
                    var createdProduct = System.Text.Json.JsonSerializer.Deserialize<Product>(productJson, _jsonOptions);
                    return new ApiResponse<Product> { Product = createdProduct, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Product created successfully" : "Product created successfully" };
                }
                return new ApiResponse<Product> { Message = "Failed to parse response" };
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
                var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("product"))
                {
                    var productJson = System.Text.Json.JsonSerializer.Serialize(result["product"]);
                    var updatedProduct = System.Text.Json.JsonSerializer.Deserialize<Product>(productJson, _jsonOptions);
                    return new ApiResponse<Product> { Product = updatedProduct, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Product updated successfully" : "Product updated successfully" };
                }
                return new ApiResponse<Product> { Message = "Failed to parse response" };
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
                var response = await _httpClient.DeleteAsync($"api/products/{id}");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Product deleted successfully" : "Product deleted successfully" };
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

        public async Task<List<Material>> SearchMaterialsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(code)) queryParams.Add($"code={Uri.EscapeDataString(code)}");
                if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value.ToString().ToLower()}");
                if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

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
                var response = await _httpClient.PostAsJsonAsync("api/materials", material, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("material"))
                {
                    var materialJson = System.Text.Json.JsonSerializer.Serialize(result["material"]);
                    var createdMaterial = System.Text.Json.JsonSerializer.Deserialize<Material>(materialJson, _jsonOptions);
                    return new ApiResponse<Material> { Material = createdMaterial, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Material created successfully" : "Material created successfully" };
                }
                return new ApiResponse<Material> { Message = "Failed to parse response" };
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
                var response = await _httpClient.PutAsJsonAsync($"api/materials/{id}", material, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("material"))
                {
                    var materialJson = System.Text.Json.JsonSerializer.Serialize(result["material"]);
                    var updatedMaterial = System.Text.Json.JsonSerializer.Deserialize<Material>(materialJson, _jsonOptions);
                    return new ApiResponse<Material> { Material = updatedMaterial, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Material updated successfully" : "Material updated successfully" };
                }
                return new ApiResponse<Material> { Message = "Failed to parse response" };
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
                var response = await _httpClient.DeleteAsync($"api/materials/{id}");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Material deleted successfully" : "Material deleted successfully" };
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

        public async Task<List<Warehouse>> SearchWarehousesAsync(string? name, string? type, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
                if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={Uri.EscapeDataString(type)}");
                if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value.ToString().ToLower()}");
                if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

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
                var response = await _httpClient.PostAsJsonAsync("api/warehouses", warehouse, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("warehouse"))
                {
                    var warehouseJson = System.Text.Json.JsonSerializer.Serialize(result["warehouse"]);
                    var createdWarehouse = System.Text.Json.JsonSerializer.Deserialize<Warehouse>(warehouseJson, _jsonOptions);
                    return new ApiResponse<Warehouse> { Warehouse = createdWarehouse, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Warehouse created successfully" : "Warehouse created successfully" };
                }
                return new ApiResponse<Warehouse> { Message = "Failed to parse response" };
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
                var response = await _httpClient.PutAsJsonAsync($"api/warehouses/{id}", warehouse, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("warehouse"))
                {
                    var warehouseJson = System.Text.Json.JsonSerializer.Serialize(result["warehouse"]);
                    var updatedWarehouse = System.Text.Json.JsonSerializer.Deserialize<Warehouse>(warehouseJson, _jsonOptions);
                    return new ApiResponse<Warehouse> { Warehouse = updatedWarehouse, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Warehouse updated successfully" : "Warehouse updated successfully" };
                }
                return new ApiResponse<Warehouse> { Message = "Failed to parse response" };
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
                var response = await _httpClient.DeleteAsync($"api/warehouses/{id}");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Warehouse deleted successfully" : "Warehouse deleted successfully" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Warehouse>> StopWarehouseAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/warehouses/{id}/stop", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("warehouse"))
                {
                    var warehouseJson = System.Text.Json.JsonSerializer.Serialize(result["warehouse"]);
                    var warehouse = System.Text.Json.JsonSerializer.Deserialize<Warehouse>(warehouseJson, _jsonOptions);
                    return new ApiResponse<Warehouse> { Warehouse = warehouse, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Warehouse stopped successfully" : "Warehouse stopped successfully" };
                }
                return new ApiResponse<Warehouse> { Message = "Failed to parse response" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Warehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Warehouse>> StartWarehouseAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/warehouses/{id}/start", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("warehouse"))
                {
                    var warehouseJson = System.Text.Json.JsonSerializer.Serialize(result["warehouse"]);
                    var warehouse = System.Text.Json.JsonSerializer.Deserialize<Warehouse>(warehouseJson, _jsonOptions);
                    return new ApiResponse<Warehouse> { Warehouse = warehouse, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Warehouse started successfully" : "Warehouse started successfully" };
                }
                return new ApiResponse<Warehouse> { Message = "Failed to parse response" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Warehouse> { Message = ex.Message };
            }
        }

        // Work Reports
        public async Task<List<WorkReport>> GetAllWorkReportsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/WorkReports");
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
                var response = await _httpClient.GetAsync($"api/WorkReports/{id}");
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
                var response = await _httpClient.GetAsync($"api/WorkReports/user/{userId}");
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
                var response = await _httpClient.GetAsync($"api/WorkReports/user/{userId}/active");
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
                var response = await _httpClient.PostAsJsonAsync("api/WorkReports", report, _jsonOptions);
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
            var response = await _httpClient.PostAsJsonAsync("api/WorkReports/start", request, _jsonOptions);
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
                var response = await _httpClient.PostAsJsonAsync($"api/WorkReports/{id}/finish", request, _jsonOptions);
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
                var response = await _httpClient.GetAsync("api/PartRequests");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<PartRequest>>(_jsonOptions) ?? new List<PartRequest>();
            }
            catch
            {
                return new List<PartRequest>();
            }
        }

        public async Task<List<PartRequest>> GetPartRequestsByUserAsync(int userId, bool sent = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/PartRequests/user/{userId}?sent={sent}");
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
                var response = await _httpClient.GetAsync($"api/PartRequests/{id}");
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
                var response = await _httpClient.PostAsJsonAsync("api/PartRequests", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("request"))
                {
                    var requestJson = System.Text.Json.JsonSerializer.Serialize(result["request"]);
                    var createdRequest = System.Text.Json.JsonSerializer.Deserialize<PartRequest>(requestJson, _jsonOptions);
                    return new ApiResponse<PartRequest> { Request = createdRequest, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "PartRequest created successfully" : "PartRequest created successfully" };
                }
                return new ApiResponse<PartRequest> { Message = "Failed to parse response" };
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
                var response = await _httpClient.PostAsync($"api/PartRequests/{id}/approve", null);
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
                var response = await _httpClient.PostAsJsonAsync($"api/PartRequests/{id}/reject", reason, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<PartRequest>>(_jsonOptions) ?? new ApiResponse<PartRequest>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<PartRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> CancelPartRequestAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/PartRequests/{id}/cancel", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Request cancelled" : "Request cancelled" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Shift Transfers
        public async Task<List<ShiftTransfer>> GetShiftTransfersByUserAsync(int userId, bool sent = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ShiftTransfers/user/{userId}?sent={sent}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ShiftTransfer>>(_jsonOptions) ?? new List<ShiftTransfer>();
            }
            catch
            {
                return new List<ShiftTransfer>();
            }
        }

        public async Task<List<ShiftTransfer>> GetPendingShiftTransfersAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ShiftTransfers/pending/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ShiftTransfer>>(_jsonOptions) ?? new List<ShiftTransfer>();
            }
            catch
            {
                return new List<ShiftTransfer>();
            }
        }

        public async Task<ApiResponse<ShiftTransfer>> CreateShiftTransferAsync(ShiftTransfer transfer)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ShiftTransfers", transfer, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<ShiftTransfer>>(_jsonOptions) ?? new ApiResponse<ShiftTransfer>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<ShiftTransfer> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ShiftTransfer>> ConfirmShiftTransferAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/ShiftTransfers/{id}/confirm", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<ShiftTransfer>>(_jsonOptions) ?? new ApiResponse<ShiftTransfer>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<ShiftTransfer> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> CancelShiftTransferAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/ShiftTransfers/{id}/cancel", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Transfer cancelled" : "Transfer cancelled" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Alarm Events
        public async Task<List<AlarmEvent>> GetAllAlarmEventsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/AlarmEvents");
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
                var response = await _httpClient.GetAsync($"api/AlarmEvents/user/{userId}");
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
                var response = await _httpClient.PostAsJsonAsync("api/AlarmEvents", alarmEvent, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("alarmEvent"))
                {
                    var alarmEventJson = System.Text.Json.JsonSerializer.Serialize(result["alarmEvent"]);
                    var createdAlarmEvent = System.Text.Json.JsonSerializer.Deserialize<AlarmEvent>(alarmEventJson, _jsonOptions);
                    return new ApiResponse<AlarmEvent> { AlarmEvent = createdAlarmEvent, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "AlarmEvent created successfully" : "AlarmEvent created successfully" };
                }
                return new ApiResponse<AlarmEvent> { Message = "Failed to parse response" };
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
        // Roles
        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Roles");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Role>>(_jsonOptions) ?? new List<Role>();
            }
            catch
            {
                return new List<Role>();
            }
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Roles/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Role>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        // Permissions
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/permissions");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Permission>>(_jsonOptions) ?? new List<Permission>();
            }
            catch
            {
                return new List<Permission>();
            }
        }

        public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/roles/{roleId}/permissions");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Permission>>(_jsonOptions) ?? new List<Permission>();
            }
            catch
            {
                return new List<Permission>();
            }
        }

        public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/roles/{roleId}/permissions", permissionIds, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

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

        public async Task<LogsSearchResult?> AdvancedSearchLogsAsync(
            int? userId = null, string? controller = null, string? action = null,
            string? httpMethod = null, int? statusCode = null, string? url = null,
            DateTime? startDate = null, DateTime? endDate = null,
            int? warehouseId = null, int? materialId = null, int? productId = null,
            long? minDurationMs = null, long? maxDurationMs = null,
            int pageNumber = 1, int pageSize = 100)
        {
            try
            {
                var queryParams = new List<string>();

                if (userId.HasValue) queryParams.Add($"userId={userId.Value}");
                if (!string.IsNullOrEmpty(controller)) queryParams.Add($"controller={Uri.EscapeDataString(controller)}");
                if (!string.IsNullOrEmpty(action)) queryParams.Add($"action={Uri.EscapeDataString(action)}");
                if (!string.IsNullOrEmpty(httpMethod)) queryParams.Add($"httpMethod={Uri.EscapeDataString(httpMethod)}");
                if (statusCode.HasValue) queryParams.Add($"statusCode={statusCode.Value}");
                if (!string.IsNullOrEmpty(url)) queryParams.Add($"url={Uri.EscapeDataString(url)}");
                if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
                if (materialId.HasValue) queryParams.Add($"materialId={materialId.Value}");
                if (productId.HasValue) queryParams.Add($"productId={productId.Value}");
                if (minDurationMs.HasValue) queryParams.Add($"minDurationMs={minDurationMs.Value}");
                if (maxDurationMs.HasValue) queryParams.Add($"maxDurationMs={maxDurationMs.Value}");
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/request-logs/advanced-search?{query}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine(
                        $"RequestLogs advanced-search failed: {(int)response.StatusCode} {response.ReasonPhrase} | {errorBody}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<LogsSearchResult>(_jsonOptions);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("RequestLogs advanced-search failed: exception thrown");
                return null;
            }
        }

        public async Task<byte[]?> ExportLogsAsync(
            string format,
            int? userId = null, string? controller = null, string? action = null,
            string? httpMethod = null, int? statusCode = null, string? url = null,
            DateTime? startDate = null, DateTime? endDate = null,
            int? warehouseId = null, int? materialId = null, int? productId = null,
            long? minDurationMs = null, long? maxDurationMs = null)
        {
            try
            {
                var queryParams = new List<string> { $"format={format}" };

                if (userId.HasValue) queryParams.Add($"userId={userId.Value}");
                if (!string.IsNullOrEmpty(controller)) queryParams.Add($"controller={Uri.EscapeDataString(controller)}");
                if (!string.IsNullOrEmpty(action)) queryParams.Add($"action={Uri.EscapeDataString(action)}");
                if (!string.IsNullOrEmpty(httpMethod)) queryParams.Add($"httpMethod={Uri.EscapeDataString(httpMethod)}");
                if (statusCode.HasValue) queryParams.Add($"statusCode={statusCode.Value}");
                if (!string.IsNullOrEmpty(url)) queryParams.Add($"url={Uri.EscapeDataString(url)}");
                if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
                if (materialId.HasValue) queryParams.Add($"materialId={materialId.Value}");
                if (productId.HasValue) queryParams.Add($"productId={productId.Value}");
                if (minDurationMs.HasValue) queryParams.Add($"minDurationMs={minDurationMs.Value}");
                if (maxDurationMs.HasValue) queryParams.Add($"maxDurationMs={maxDurationMs.Value}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/request-logs/export?{query}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}


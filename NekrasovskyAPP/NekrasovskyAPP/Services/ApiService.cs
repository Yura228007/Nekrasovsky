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
            return "http://192.168.0.47:9000/";
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

        public async Task<ApiResponse<object>> UnlockUserDeviceAsync(int userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/users/{userId}/unlock-device", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "OK" : "OK" };
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
                var jsonString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                    var msg = result?.ContainsKey("message") == true ? result["message"]?.ToString() : null;
                    var fullMsg = msg ?? response.ReasonPhrase ?? "Ошибка удаления продукта";
                    return new ApiResponse<object> { Message = fullMsg.Contains("Ошибка", StringComparison.OrdinalIgnoreCase) ? fullMsg : "Ошибка: " + fullMsg };
                }
                var okResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object> { Message = okResult?.ContainsKey("message") == true ? okResult["message"]?.ToString() ?? "Product deleted successfully" : "Product deleted successfully" };
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

        public async Task<ApiResponse<Material>> AddMaterialAsync(Material material, int? quantity = null, string? measuringUnit = null, int? warehouseId = null)
        {
            try
            {
                var url = "api/materials";
                var queryParams = new List<string>();
                if (quantity.HasValue)
                    queryParams.Add($"quantity={quantity.Value}");
                if (!string.IsNullOrWhiteSpace(measuringUnit))
                    queryParams.Add($"measuringUnit={Uri.EscapeDataString(measuringUnit)}");
                if (warehouseId.HasValue)
                    queryParams.Add($"warehouseId={warehouseId.Value}");
                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);
                
                var response = await _httpClient.PostAsJsonAsync(url, material, _jsonOptions);
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

        // Filling Warehouses
        public async Task<List<FillingWarehouse>> GetAllFillingWarehousesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/filling-warehouses");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<FillingWarehouse>>(_jsonOptions) ?? new List<FillingWarehouse>();
            }
            catch
            {
                return new List<FillingWarehouse>();
            }
        }

        public async Task<FillingWarehouse?> GetFillingByMaterialAsync(int warehouseId, int materialId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/filling-warehouses/{warehouseId}/{materialId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<FillingWarehouse>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<FillingWarehouse?> GetFillingByProductAsync(int warehouseId, int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/filling-warehouses/{warehouseId}/product/{productId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<FillingWarehouse>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<FillingWarehouse>> GetFillingsByWarehouseAsync(int warehouseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/filling-warehouses/warehouse/{warehouseId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<FillingWarehouse>>(_jsonOptions) ?? new List<FillingWarehouse>();
            }
            catch
            {
                return new List<FillingWarehouse>();
            }
        }

        public async Task<List<FillingWarehouse>> GetFillingsByMaterialAsync(int materialId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/filling-warehouses/material/{materialId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<FillingWarehouse>>(_jsonOptions) ?? new List<FillingWarehouse>();
            }
            catch
            {
                return new List<FillingWarehouse>();
            }
        }

        public async Task<List<FillingWarehouse>> GetFillingsByProductAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/filling-warehouses/product/{productId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<FillingWarehouse>>(_jsonOptions) ?? new List<FillingWarehouse>();
            }
            catch
            {
                return new List<FillingWarehouse>();
            }
        }

        public async Task<ApiResponse<FillingWarehouse>> AddFillingWarehouseAsync(FillingWarehouse filling)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/filling-warehouses", filling, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<FillingWarehouse>>(_jsonOptions) ?? new ApiResponse<FillingWarehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<FillingWarehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<FillingWarehouse>> EditFillingWarehouseByMaterialAsync(int warehouseId, int materialId, FillingWarehouse filling)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/filling-warehouses/edit/{warehouseId}/{materialId}", filling, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<FillingWarehouse>>(_jsonOptions) ?? new ApiResponse<FillingWarehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<FillingWarehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<FillingWarehouse>> EditFillingWarehouseByProductAsync(int warehouseId, int productId, FillingWarehouse filling)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/filling-warehouses/edit-product/{warehouseId}/{productId}", filling, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<FillingWarehouse>>(_jsonOptions) ?? new ApiResponse<FillingWarehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<FillingWarehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteFillingWarehouseByMaterialAsync(int warehouseId, int materialId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/filling-warehouses/delete/{warehouseId}/{materialId}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions) ?? new ApiResponse<object>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteFillingWarehouseByProductAsync(int warehouseId, int productId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/filling-warehouses/delete-product/{warehouseId}/{productId}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions) ?? new ApiResponse<object>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<FillingWarehouse>> UpdateFillingQuantityByMaterialAsync(FillingWarehouse filling)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/filling-warehouses/update-quantity", filling, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<FillingWarehouse>>(_jsonOptions) ?? new ApiResponse<FillingWarehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<FillingWarehouse> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<FillingWarehouse>> UpdateFillingQuantityByProductAsync(FillingWarehouse filling)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/filling-warehouses/update-quantity-product", filling, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<FillingWarehouse>>(_jsonOptions) ?? new ApiResponse<FillingWarehouse>();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<FillingWarehouse> { Message = ex.Message };
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
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = response.ReasonPhrase ?? "Ошибка создания запроса";
                    try
                    {
                        var err = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _jsonOptions);
                        if (err?.ContainsKey("message") == true && err["message"]?.ToString() is { } msg)
                            message = msg;
                    }
                    catch { /* ignore */ }
                    return new ApiResponse<PartRequest> { Message = message };
                }
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("request"))
                {
                    var requestJson = System.Text.Json.JsonSerializer.Serialize(result["request"]);
                    var createdRequest = System.Text.Json.JsonSerializer.Deserialize<PartRequest>(requestJson, _jsonOptions);
                    return new ApiResponse<PartRequest> { Request = createdRequest, Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Запрос создан" : "Запрос создан" };
                }
                return new ApiResponse<PartRequest> { Message = "Не удалось разобрать ответ сервера" };
            }
            catch (Exception ex)
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
                var response = await _httpClient.GetAsync($"api/UserPermissions/user/{userId}");
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
                var response = await _httpClient.GetAsync($"api/UserPermissions/check?userId={userId}&permissionCode={Uri.EscapeDataString(permissionCode)}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>(_jsonOptions);
                return result?.GetValueOrDefault("hasPermission", false) ?? false;
            }
            catch
            {
                return false;
            }
        }

        // Responsibilities
        public async Task<List<Responsibility>> GetResponsibilitiesByUserAsync(int userId, bool activeOnly = true)
        {
            var activeOnlyParam = activeOnly.ToString().ToLower();
            var responsibilities = await TryGetResponsibilitiesAsync(
                                       $"api/responsibilities/user/{userId}?activeOnly={activeOnlyParam}")
                                   ?? await TryGetResponsibilitiesAsync(
                                       $"api/Responsibilities/user/{userId}?activeOnly={activeOnlyParam}")
                                   ?? new List<Responsibility>();

            if (!activeOnly || responsibilities.Count > 0)
            {
                return responsibilities;
            }

            // Fallback for legacy data where active flags may be inconsistent.
            var allResponsibilities = await TryGetResponsibilitiesAsync(
                                          $"api/responsibilities/user/{userId}?activeOnly=false")
                                      ?? await TryGetResponsibilitiesAsync(
                                          $"api/Responsibilities/user/{userId}?activeOnly=false")
                                      ?? new List<Responsibility>();

            return allResponsibilities
                .Where(r => r.IsActive || r.ReleasedAt == null)
                .ToList();
        }

        private async Task<List<Responsibility>?> TryGetResponsibilitiesAsync(string relativeUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(relativeUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<List<Responsibility>>(_jsonOptions)
                    ?? new List<Responsibility>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<Responsibility?> GetResponsibilityByMaterialAsync(int materialId, bool activeOnly = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/responsibilities/material/{materialId}?activeOnly={activeOnly.ToString().ToLower()}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Responsibility>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Responsibility?> GetResponsibilityByProductAsync(int productId, bool activeOnly = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/responsibilities/product/{productId}?activeOnly={activeOnly.ToString().ToLower()}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Responsibility>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ResponsibilityStockItem>> GetResponsibilityStockAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/responsibilities/user/{userId}/stock");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ResponsibilityStockItem>>(_jsonOptions) ?? new List<ResponsibilityStockItem>();
            }
            catch
            {
                return new List<ResponsibilityStockItem>();
            }
        }

        public async Task<List<ResponsibilityAssignment>> GetActiveMaterialAssignmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/responsibilities/materials/active");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ResponsibilityAssignment>>(_jsonOptions) ?? new List<ResponsibilityAssignment>();
            }
            catch
            {
                return new List<ResponsibilityAssignment>();
            }
        }

        public async Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/responsibilities/products/active");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ResponsibilityAssignment>>(_jsonOptions) ?? new List<ResponsibilityAssignment>();
            }
            catch
            {
                return new List<ResponsibilityAssignment>();
            }
        }

        public async Task<ApiResponse<Responsibility>> AssignMaterialResponsibilityAsync(int materialId, int userId, int? quantity = null, string? measuringUnit = null)
        {
            try
            {
                var payload = new { userId, quantity, measuringUnit };
                var response = await _httpClient.PostAsJsonAsync($"api/responsibilities/material/{materialId}/assign", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("responsibility"))
                {
                    var responsibilityJson = System.Text.Json.JsonSerializer.Serialize(result["responsibility"]);
                    var responsibility = System.Text.Json.JsonSerializer.Deserialize<Responsibility>(responsibilityJson, _jsonOptions);
                    return new ApiResponse<Responsibility>
                    {
                        Responsibility = responsibility,
                        Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Responsibility assigned" : "Responsibility assigned"
                    };
                }
                return new ApiResponse<Responsibility> { Message = "Failed to parse response" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Responsibility> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Responsibility>> AssignProductResponsibilityAsync(int productId, int userId, int? quantity = null, string? measuringUnit = null)
        {
            try
            {
                var payload = new { userId, quantity, measuringUnit };
                var response = await _httpClient.PostAsJsonAsync($"api/responsibilities/product/{productId}/assign", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("responsibility"))
                {
                    var responsibilityJson = System.Text.Json.JsonSerializer.Serialize(result["responsibility"]);
                    var responsibility = System.Text.Json.JsonSerializer.Deserialize<Responsibility>(responsibilityJson, _jsonOptions);
                    return new ApiResponse<Responsibility>
                    {
                        Responsibility = responsibility,
                        Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Responsibility assigned" : "Responsibility assigned"
                    };
                }
                return new ApiResponse<Responsibility> { Message = "Failed to parse response" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Responsibility> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> TransferMaterialResponsibilityFillingAsync(int warehouseId, int materialId, int fromUserId, int toUserId, int? quantityToTransfer = null)
        {
            try
            {
                var payload = new { warehouseId, materialId, fromUserId, toUserId, quantityToTransfer };
                var response = await _httpClient.PostAsJsonAsync("api/responsibilities/filling/material/transfer", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object>
                {
                    Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Responsibility transferred" : "Responsibility transferred"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> TransferProductResponsibilityFillingAsync(int warehouseId, int productId, int fromUserId, int toUserId, int? quantityToTransfer = null)
        {
            try
            {
                var payload = new { warehouseId, productId, fromUserId, toUserId, quantityToTransfer };
                var response = await _httpClient.PostAsJsonAsync("api/responsibilities/filling/product/transfer", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object>
                {
                    Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Responsibility transferred" : "Responsibility transferred"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> TransferBatchResponsibilityFillingAsync(int batchId, int fromUserId, int toUserId, int? quantityToTransfer = null)
        {
            try
            {
                var payload = new { batchId, fromUserId, toUserId, quantityToTransfer };
                var response = await _httpClient.PostAsJsonAsync("api/responsibilities/filling/batch/transfer", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object>
                {
                    Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Responsibility transferred" : "Responsibility transferred"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> ReleaseBatchResponsibilityAsync(int batchId, int userId)
        {
            try
            {
                var payload = new { batchId, userId };
                var response = await _httpClient.PostAsJsonAsync("api/responsibilities/filling/batch/release", payload, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object>
                {
                    Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Responsibility released" : "Responsibility released"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> ReleaseMaterialResponsibilityAsync(int materialId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/responsibilities/material/{materialId}/release", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object>
                {
                    Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Responsibility released" : "Responsibility released"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = $"Ошибка: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<object>> ReleaseProductResponsibilityAsync(int productId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/responsibilities/product/{productId}/release", null);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                return new ApiResponse<object>
                {
                    Message = result?.ContainsKey("message") == true ? result["message"]?.ToString() ?? "Responsibility released" : "Responsibility released"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = $"Ошибка: {ex.Message}" };
            }
        }

        // Reprocessing
        public async Task<ApiResponse<Reprocessing>> CreateReprocessingAsync(ReprocessingCreateRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/reprocessings", request, _jsonOptions);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _jsonOptions);
                        var message = errorObj?.ContainsKey("message") == true 
                            ? errorObj["message"]?.ToString() 
                            : response.ReasonPhrase ?? "Ошибка переработки";
                        
                        // Если есть errors (ModelState), добавляем их
                        if (errorObj?.ContainsKey("errors") == true)
                        {
                            var errorsJson = System.Text.Json.JsonSerializer.Serialize(errorObj["errors"]);
                            message += $"\nДетали: {errorsJson}";
                        }
                        
                        return new ApiResponse<Reprocessing> { Message = message ?? "Ошибка переработки" };
                    }
                    catch
                    {
                        return new ApiResponse<Reprocessing> { Message = $"Ошибка {(int)response.StatusCode}: {errorContent}" };
                    }
                }
                
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, _jsonOptions);
                if (result != null && result.ContainsKey("reprocessing"))
                {
                    var payload = System.Text.Json.JsonSerializer.Serialize(result["reprocessing"]);
                    var reprocessing = System.Text.Json.JsonSerializer.Deserialize<Reprocessing>(payload, _jsonOptions);
                    return new ApiResponse<Reprocessing>
                    {
                        Reprocessing = reprocessing,
                        Message = result.ContainsKey("message") ? result["message"]?.ToString() ?? "Reprocessing created successfully" : "Reprocessing created successfully"
                    };
                }
                return new ApiResponse<Reprocessing> { Message = "Failed to parse response" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Reprocessing> { Message = $"Ошибка сети: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Reprocessing> { Message = $"Неожиданная ошибка: {ex.Message}" };
            }
        }

        // Disposal (Утиль)
        public async Task<ApiResponse<object>> ProcessDisposalAsync(DisposalProcessRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/disposal/process", request, _jsonOptions);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _jsonOptions);
                    var message = errorObj?.ContainsKey("message") == true ? errorObj["message"]?.ToString() : response.ReasonPhrase ?? "Ошибка списания";
                    return new ApiResponse<object> { Message = message ?? "Ошибка обработки утиля" };
                }
                return new ApiResponse<object> { Message = "Утиль обработан успешно" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Product Outputs
        public async Task<ProductOutputOptionsResponse?> GetProductOutputOptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/product-outputs/options");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ProductOutputOptionsResponse>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ProductOutput>> GetAllProductOutputsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/product-outputs");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ProductOutput>>(_jsonOptions) ?? new List<ProductOutput>();
            }
            catch
            {
                return new List<ProductOutput>();
            }
        }

        public async Task<List<ProductOutput>> GetProductOutputsByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/product-outputs/user/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ProductOutput>>(_jsonOptions) ?? new List<ProductOutput>();
            }
            catch
            {
                return new List<ProductOutput>();
            }
        }

        public async Task<ProductOutput?> GetProductOutputByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/product-outputs/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ProductOutput>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<ProductOutput>> AddProductOutputAsync(ProductOutput output)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/product-outputs", output, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ProductOutput>(_jsonOptions);
                return new ApiResponse<ProductOutput> { Output = result, Message = "Выпуск продукции добавлен" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<ProductOutput> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ProductOutput>> EditProductOutputAsync(int id, ProductOutput output)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/product-outputs/{id}", output, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ProductOutput>(_jsonOptions);
                return new ApiResponse<ProductOutput> { Output = result, Message = "Выпуск продукции обновлен" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<ProductOutput> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteProductOutputAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/product-outputs/{id}");
                response.EnsureSuccessStatusCode();
                return new ApiResponse<object> { Message = "Выпуск продукции удален" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Machines
        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/machines");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Machine>>(_jsonOptions) ?? new List<Machine>();
            }
            catch
            {
                return new List<Machine>();
            }
        }

        public async Task<List<Machine>> GetActiveMachinesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/machines/active");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Machine>>(_jsonOptions) ?? new List<Machine>();
            }
            catch
            {
                return new List<Machine>();
            }
        }

        public async Task<Machine?> GetMachineByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/machines/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Machine>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<Machine>> AddMachineAsync(Machine machine)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/machines", machine, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Machine>(_jsonOptions);
                return new ApiResponse<Machine> { Machine = result, Message = "Станок добавлен" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Machine> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<Machine>> EditMachineAsync(int id, Machine machine)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/machines/{id}", machine, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Machine>(_jsonOptions);
                return new ApiResponse<Machine> { Machine = result, Message = "Станок обновлен" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<Machine> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteMachineAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/machines/{id}");
                response.EnsureSuccessStatusCode();
                return new ApiResponse<object> { Message = "Станок удален" };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
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

        public async Task<List<HistoryEvent>?> GetHistoryAsync(
            int? userId = null,
            int? relatedUserId = null,
            string? action = null,
            string? entityType = null,
            int? warehouseId = null,
            int? materialId = null,
            int? productId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var queryParams = new List<string>();

                if (userId.HasValue) queryParams.Add($"userId={userId.Value}");
                if (relatedUserId.HasValue) queryParams.Add($"relatedUserId={relatedUserId.Value}");
                if (!string.IsNullOrEmpty(action)) queryParams.Add($"action={Uri.EscapeDataString(action)}");
                if (!string.IsNullOrEmpty(entityType)) queryParams.Add($"entityType={Uri.EscapeDataString(entityType)}");
                if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");
                if (materialId.HasValue) queryParams.Add($"materialId={materialId.Value}");
                if (productId.HasValue) queryParams.Add($"productId={productId.Value}");
                if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:O}");
                if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:O}");

                var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
                var response = await _httpClient.GetAsync($"api/history{query}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<List<HistoryEvent>>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        // Shift Reports
        public async Task<List<ShiftReport>> GetShiftReportsByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ShiftReports/user/{userId}?requestingUserId={_currentUserId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ShiftReport>>(_jsonOptions) ?? new List<ShiftReport>();
            }
            catch
            {
                return new List<ShiftReport>();
            }
        }

        public async Task<ShiftReport?> GetShiftReportByWorkReportIdAsync(int workReportId)
        {
            var response = await _httpClient.GetAsync($"api/ShiftReports/by-work-report/{workReportId}?requestingUserId={_currentUserId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ShiftReport>(_jsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            string? serverMessage = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("detail", out var detail))
                    serverMessage = detail.GetString();
                if (string.IsNullOrEmpty(serverMessage) && doc.RootElement.TryGetProperty("message", out var msg))
                    serverMessage = msg.GetString();
            }
            catch { /* ignore parse */ }
            throw new HttpRequestException(serverMessage ?? response.ReasonPhrase ?? "Отчет смены не найден");
        }

        public async Task<byte[]?> DownloadShiftReportAsync(int reportId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ShiftReports/{reportId}/download?requestingUserId={_currentUserId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }

        // Product Batches
        public async Task<List<ProductBatch>> GetAllProductBatchesAsync()
        {
            var response = await _httpClient.GetAsync("api/product-batches");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductBatch>>(_jsonOptions) ?? new List<ProductBatch>();
            }
            return new List<ProductBatch>();
        }

        public async Task<ProductBatch?> GetProductBatchByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/product-batches/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductBatch>(_jsonOptions);
            }
            return null;
        }

        public async Task<List<ProductBatch>> GetProductBatchesByProductAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"api/product-batches/product/{productId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductBatch>>(_jsonOptions) ?? new List<ProductBatch>();
            }
            return new List<ProductBatch>();
        }

        public async Task<List<ProductBatch>> GetProductBatchesByWarehouseAsync(int warehouseId)
        {
            var response = await _httpClient.GetAsync($"api/product-batches/warehouse/{warehouseId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductBatch>>(_jsonOptions) ?? new List<ProductBatch>();
            }
            return new List<ProductBatch>();
        }

        public async Task<List<ProductBatch>> GetProductBatchesByUserAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/product-batches/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductBatch>>(_jsonOptions) ?? new List<ProductBatch>();
            }
            return new List<ProductBatch>();
        }

        public async Task<ApiResponse<ProductBatch>> CreateProductBatchAsync(ProductBatch batch)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/product-batches", batch, _jsonOptions);
                var result = await response.Content.ReadFromJsonAsync<ProductBatch>(_jsonOptions);
                return new ApiResponse<ProductBatch> { Batch = result, Message = "Партия создана" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductBatch> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ProductBatch>> UpdateProductBatchAsync(int id, ProductBatch batch)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/product-batches/{id}", batch, _jsonOptions);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = response.ReasonPhrase ?? "Ошибка обновления партии";
                    try
                    {
                        var err = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _jsonOptions);
                        if (err?.ContainsKey("message") == true && err["message"]?.ToString() is { } msg)
                            message = msg;
                    }
                    catch { /* ignore */ }
                    return new ApiResponse<ProductBatch> { Message = message };
                }
                var result = await response.Content.ReadFromJsonAsync<ProductBatch>(_jsonOptions);
                return new ApiResponse<ProductBatch> { Batch = result, Message = "Партия обновлена" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductBatch> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteProductBatchAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/product-batches/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = response.ReasonPhrase ?? "Ошибка удаления партии";
                    try
                    {
                        var err = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _jsonOptions);
                        if (err?.ContainsKey("message") == true && err["message"]?.ToString() is { } msg)
                            message = msg;
                    }
                    catch { /* ignore */ }
                    return new ApiResponse<object> { Message = message };
                }
                return new ApiResponse<object> { Message = "Партия успешно удалена" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }

        // Product Movement Requests
        public async Task<List<ProductMovementRequest>> GetAllProductMovementRequestsAsync()
        {
            var response = await _httpClient.GetAsync("api/product-movement-requests");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductMovementRequest>>(_jsonOptions) ?? new List<ProductMovementRequest>();
            }
            return new List<ProductMovementRequest>();
        }

        public async Task<ProductMovementRequest?> GetProductMovementRequestByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/product-movement-requests/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductMovementRequest>(_jsonOptions);
            }
            return null;
        }

        public async Task<List<ProductMovementRequest>> GetProductMovementRequestsByStatusAsync(ProductMovementStatus status)
        {
            var response = await _httpClient.GetAsync($"api/product-movement-requests/status/{(int)status}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductMovementRequest>>(_jsonOptions) ?? new List<ProductMovementRequest>();
            }
            return new List<ProductMovementRequest>();
        }

        public async Task<List<ProductMovementRequest>> GetSentProductMovementRequestsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/product-movement-requests/user/{userId}/sent");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductMovementRequest>>(_jsonOptions) ?? new List<ProductMovementRequest>();
            }
            return new List<ProductMovementRequest>();
        }

        public async Task<List<ProductMovementRequest>> GetReceivedProductMovementRequestsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/product-movement-requests/user/{userId}/received");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductMovementRequest>>(_jsonOptions) ?? new List<ProductMovementRequest>();
            }
            return new List<ProductMovementRequest>();
        }

        public async Task<ApiResponse<ProductMovementRequest>> CreateProductMovementRequestAsync(ProductMovementRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/product-movement-requests", request, _jsonOptions);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = response.ReasonPhrase ?? "Ошибка создания заявки";
                    try
                    {
                        var err = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _jsonOptions);
                        if (err?.ContainsKey("message") == true && err["message"]?.ToString() is { } msg)
                            message = msg;
                    }
                    catch { /* ignore */ }
                    return new ApiResponse<ProductMovementRequest> { Message = message };
                }
                var result = await response.Content.ReadFromJsonAsync<ProductMovementRequest>(_jsonOptions);
                return new ApiResponse<ProductMovementRequest> { Request = result, Message = "Заявка создана" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductMovementRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ProductMovementRequest>> UpdateProductMovementRequestAsync(int id, ProductMovementRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/product-movement-requests/{id}", request, _jsonOptions);
                var result = await response.Content.ReadFromJsonAsync<ProductMovementRequest>(_jsonOptions);
                return new ApiResponse<ProductMovementRequest> { Request = result, Message = "Заявка обновлена" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductMovementRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ProductMovementRequest>> ApproveProductMovementRequestAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/product-movement-requests/{id}/approve", null);
                var result = await response.Content.ReadFromJsonAsync<ProductMovementRequest>(_jsonOptions);
                return new ApiResponse<ProductMovementRequest> { Request = result, Message = "Заявка одобрена" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductMovementRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ProductMovementRequest>> RejectProductMovementRequestAsync(int id, string? reason)
        {
            try
            {
                var content = new { Reason = reason };
                var response = await _httpClient.PostAsJsonAsync($"api/product-movement-requests/{id}/reject", content, _jsonOptions);
                var result = await response.Content.ReadFromJsonAsync<ProductMovementRequest>(_jsonOptions);
                return new ApiResponse<ProductMovementRequest> { Request = result, Message = "Заявка отклонена" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductMovementRequest> { Message = ex.Message };
            }
        }

        public async Task<ApiResponse<object>> DeleteProductMovementRequestAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/product-movement-requests/{id}");
                return new ApiResponse<object> { Message = response.IsSuccessStatusCode ? "Заявка удалена" : "Ошибка удаления" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }
    }
}


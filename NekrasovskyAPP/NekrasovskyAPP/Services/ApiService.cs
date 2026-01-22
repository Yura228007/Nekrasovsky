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
        private int? _currentUserId;

        private static string GetBaseUrl()
        {
#if ANDROID
            return "http://192.168.1.121:9000/";
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

        public void SetCurrentUserId(int? userId)
        {
            _currentUserId = userId;
            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");

            if (userId.HasValue)
            {
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId.Value.ToString());
            }
        }

        // ===================== USERS =====================

        public async Task<List<User>> GetAllUsersAsync() =>
            await SafeGetAsync<List<User>>("api/users") ?? new();

        public async Task<User?> GetUserByIdAsync(int id) =>
            await SafeGetAsync<User>($"api/users/{id}");

        public async Task<List<User>> SearchUsersAsync(string? name, string? surname)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
            if (!string.IsNullOrEmpty(surname)) query.Add($"surname={Uri.EscapeDataString(surname)}");

            return await SafeGetAsync<List<User>>($"api/users/search?{string.Join("&", query)}") ?? new();
        }

        public async Task<ApiResponse<User>> AddUserAsync(User user) =>
            await SafePostAsync<User>("api/users", user);

        public async Task<ApiResponse<User>> EditUserAsync(int id, User user) =>
            await SafePutAsync<User>($"api/users/{id}", user);

        public async Task<ApiResponse<object>> DeleteUserAsync(int id) =>
            await SafeDeleteAsync($"api/users/{id}");

        // ===================== WORK REPORTS =====================

        public async Task<List<WorkReport>> GetAllWorkReportsAsync() =>
            await SafeGetAsync<List<WorkReport>>("api/work-reports") ?? new();

        public async Task<WorkReport?> GetWorkReportByIdAsync(int id) =>
            await SafeGetAsync<WorkReport>($"api/work-reports/{id}");

        public async Task<List<WorkReport>> GetWorkReportsByUserAsync(int userId) =>
            await SafeGetAsync<List<WorkReport>>($"api/work-reports/user/{userId}") ?? new();

        public async Task<List<WorkReport>> GetActiveWorkReportsAsync(int userId) =>
            await SafeGetAsync<List<WorkReport>>($"api/work-reports/user/{userId}/active") ?? new();

        public async Task<ApiResponse<WorkReport>> AddWorkReportAsync(WorkReport report) =>
            await SafePostAsync<WorkReport>("api/work-reports", report);

        public async Task<ApiResponse<WorkReport>> StartWorkAsync(StartWorkRequest request) =>
            await SafePostAsync<WorkReport>("api/work-reports/start", request);

        public async Task<ApiResponse<WorkReport>> FinishWorkAsync(int id, FinishWorkRequest? request) =>
            await SafePostAsync<WorkReport>($"api/work-reports/{id}/finish", request);

        // ===================== HELPERS =====================

        private async Task<T?> SafeGetAsync<T>(string url)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(url, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        private async Task<ApiResponse<T>> SafePostAsync<T>(string url, object? body)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, body, _jsonOptions);
                var json = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode
                    ? JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions) ?? new()
                    : new ApiResponse<T> { Message = json };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Message = ex.Message };
            }
        }

        private async Task<ApiResponse<T>> SafePutAsync<T>(string url, object body)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(url, body, _jsonOptions);
                var json = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode
                    ? JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions) ?? new()
                    : new ApiResponse<T> { Message = json };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Message = ex.Message };
            }
        }

        private async Task<ApiResponse<object>> SafeDeleteAsync(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode
                    ? new ApiResponse<object> { Message = "Deleted successfully" }
                    : new ApiResponse<object> { Message = json };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Message = ex.Message };
            }
        }
    }
}

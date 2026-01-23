using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public MainViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Users = new ObservableCollection<User>();
            Products = new ObservableCollection<Product>();
            Materials = new ObservableCollection<Material>();
            Warehouses = new ObservableCollection<Warehouse>();
            Roles = new ObservableCollection<Role>();
        }

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Product> Products { get; }
        public ObservableCollection<Material> Materials { get; }
        public ObservableCollection<Warehouse> Warehouses { get; }
        public ObservableCollection<Role> Roles { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var users = await _apiService.GetAllUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки пользователей: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var products = await _apiService.GetAllProductsAsync();
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки продуктов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadMaterialsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var materials = await _apiService.GetAllMaterialsAsync();
                Materials.Clear();
                foreach (var material in materials)
                {
                    Materials.Add(material);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки материалов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadWarehousesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var warehouses = await _apiService.GetAllWarehousesAsync();
                Warehouses.Clear();
                foreach (var warehouse in warehouses)
                {
                    Warehouses.Add(warehouse);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки складов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadRolesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var roles = await _apiService.GetAllRolesAsync();
                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки ролей: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchUsersAsync(string? name, string? surname)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var users = await _apiService.SearchUsersAsync(name, surname);
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска пользователей: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchProductsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var products = await _apiService.SearchProductsAsync(name, code, isActive, sortBy);
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска продуктов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchMaterialsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var materials = await _apiService.SearchMaterialsAsync(name, code, isActive, sortBy);
                Materials.Clear();
                foreach (var material in materials)
                {
                    Materials.Add(material);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска материалов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchWarehousesAsync(string? name, string? type, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var warehouses = await _apiService.SearchWarehousesAsync(name, type, isActive, sortBy);
                Warehouses.Clear();
                foreach (var warehouse in warehouses)
                {
                    Warehouses.Add(warehouse);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска складов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // User management methods
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.AddUserAsync(user);
                if (response.GetData() != null)
                {
                    await LoadUsersAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка создания пользователя";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания пользователя: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, User user)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.EditUserAsync(id, user);
                if (response.GetData() != null)
                {
                    await LoadUsersAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка обновления пользователя";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления пользователя: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.DeleteUserAsync(id);
                // Проверяем успешность по сообщению - если есть сообщение и оно не содержит "error" или "ошибка", считаем успешным
                if (!string.IsNullOrEmpty(response.Message) && 
                    !response.Message.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !response.Message.Contains("ошибка", StringComparison.OrdinalIgnoreCase) &&
                    !response.Message.Contains("Error", StringComparison.OrdinalIgnoreCase))
                {
                    await LoadUsersAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка удаления пользователя";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления пользователя: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Product management methods
        public async Task<bool> CreateProductAsync(Product product)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.AddProductAsync(product);
                if (response.GetData() != null)
                {
                    await LoadProductsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка создания продукта";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания продукта: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> UpdateProductAsync(int id, Product product)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.EditProductAsync(id, product);
                if (response.GetData() != null)
                {
                    await LoadProductsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка обновления продукта";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления продукта: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.DeleteProductAsync(id);
                if (response.GetData() != null || string.IsNullOrEmpty(response.Message))
                {
                    await LoadProductsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка удаления продукта";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления продукта: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Material management methods
        public async Task<bool> CreateMaterialAsync(Material material)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.AddMaterialAsync(material);
                if (response.GetData() != null)
                {
                    await LoadMaterialsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка создания материала";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания материала: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> UpdateMaterialAsync(int id, Material material)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.EditMaterialAsync(id, material);
                if (response.GetData() != null)
                {
                    await LoadMaterialsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка обновления материала";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления материала: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.DeleteMaterialAsync(id);
                if (response.GetData() != null || string.IsNullOrEmpty(response.Message))
                {
                    await LoadMaterialsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка удаления материала";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления материала: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Warehouse management methods
        public async Task<bool> CreateWarehouseAsync(Warehouse warehouse)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.AddWarehouseAsync(warehouse);
                if (response.GetData() != null)
                {
                    await LoadWarehousesAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка создания склада";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания склада: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> UpdateWarehouseAsync(int id, Warehouse warehouse)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.EditWarehouseAsync(id, warehouse);
                if (response.GetData() != null)
                {
                    await LoadWarehousesAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка обновления склада";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления склада: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> DeleteWarehouseAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.DeleteWarehouseAsync(id);
                if (response.GetData() != null || string.IsNullOrEmpty(response.Message))
                {
                    await LoadWarehousesAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка удаления склада";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления склада: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> StopWarehouseAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.StopWarehouseAsync(id);
                if (response.GetData() != null)
                {
                    await LoadWarehousesAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка остановки склада";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка остановки склада: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> StartWarehouseAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.StartWarehouseAsync(id);
                if (response.GetData() != null)
                {
                    await LoadWarehousesAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка запуска склада";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка запуска склада: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


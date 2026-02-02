using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int _materialSearchVersion;
        private int _productSearchVersion;
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private bool _showResponsibility;

        public MainViewModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
            Users = new ObservableCollection<User>();
            Products = new ObservableCollection<ProductDisplayItem>();
            ProductsForCatalog = new ObservableCollection<Product>();
            Materials = new ObservableCollection<MaterialDisplayItem>();
            Warehouses = new ObservableCollection<Warehouse>();
            Roles = new ObservableCollection<Role>();
            ProductBatches = new ObservableCollection<ProductBatch>();
        }

        public IApiService ApiService => _apiService;

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<ProductDisplayItem> Products { get; }
        /// <summary>Простой список продуктов для каталога (по одному на продукт).</summary>
        public ObservableCollection<Product> ProductsForCatalog { get; }
        public ObservableCollection<MaterialDisplayItem> Materials { get; }
        public ObservableCollection<Warehouse> Warehouses { get; }
        public ObservableCollection<Role> Roles { get; }
        public ObservableCollection<ProductBatch> ProductBatches { get; }

        public bool ShowResponsibility
        {
            get => _showResponsibility;
            private set
            {
                _showResponsibility = value;
                OnPropertyChanged();
            }
        }

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
                var filtered = await FilterProductsByResponsibilityAsync(products);
                await ApplyProductStockAsync(filtered);
                ShowResponsibility = true;
                var displayItems = await CreateProductDisplayItemsAsync(filtered);
                Products.Clear();
                foreach (var item in displayItems)
                {
                    Products.Add(item);
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
                var filtered = await FilterMaterialsByResponsibilityAsync(materials);
                await ApplyMaterialStockAsync(filtered);
                ShowResponsibility = true;
                var displayItems = await CreateMaterialDisplayItemsAsync(filtered);
                Materials.Clear();
                foreach (var item in displayItems)
                {
                    Materials.Add(item);
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
                var searchVersion = System.Threading.Interlocked.Increment(ref _productSearchVersion);
                var products = await _apiService.SearchProductsAsync(name, code, isActive, sortBy);
                if (searchVersion != _productSearchVersion)
                {
                    return;
                }
                var filtered = await FilterProductsByResponsibilityAsync(products);
                await ApplyProductStockAsync(filtered);
                ShowResponsibility = true;
                var displayItems = await CreateProductDisplayItemsAsync(filtered);
                if (searchVersion != _productSearchVersion)
                {
                    return;
                }
                Products.Clear();
                foreach (var item in displayItems)
                {
                    Products.Add(item);
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

        /// <summary>Загружает продукты для каталога (по одному элементу на продукт, без ответственности).</summary>
        public async Task LoadProductsForCatalogAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var products = await _apiService.GetAllProductsAsync();
                var filtered = await FilterProductsByResponsibilityAsync(products);
                ProductsForCatalog.Clear();
                foreach (var p in filtered)
                {
                    ProductsForCatalog.Add(p);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки каталога: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>Поиск продуктов для каталога.</summary>
        public async Task SearchProductsForCatalogAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var products = await _apiService.SearchProductsAsync(name, code, isActive, sortBy);
                var filtered = await FilterProductsByResponsibilityAsync(products);
                ProductsForCatalog.Clear();
                foreach (var p in filtered)
                {
                    ProductsForCatalog.Add(p);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска каталога: {ex.Message}";
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
                var searchVersion = System.Threading.Interlocked.Increment(ref _materialSearchVersion);
                var materials = await _apiService.SearchMaterialsAsync(name, code, isActive, sortBy);
                if (searchVersion != _materialSearchVersion)
                {
                    return;
                }
                var filtered = await FilterMaterialsByResponsibilityAsync(materials);
                await ApplyMaterialStockAsync(filtered);
                ShowResponsibility = true;
                var displayItems = await CreateMaterialDisplayItemsAsync(filtered);
                if (searchVersion != _materialSearchVersion)
                {
                    return;
                }
                Materials.Clear();
                foreach (var item in displayItems)
                {
                    Materials.Add(item);
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

        private async Task<List<Material>> FilterMaterialsByResponsibilityAsync(IEnumerable<Material> materials)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return new List<Material>();
            }

            if (await IsPrivilegedUserAsync())
            {
                return materials.ToList();
            }

            var hasManageResponsibility = await _apiService.CheckPermissionAsync(currentUser.Id, "ManageResponsibility");
            if (hasManageResponsibility)
            {
                return materials.ToList();
            }

            var responsibilities = await _apiService.GetResponsibilitiesByUserAsync(currentUser.Id, true);
            var allowedIds = responsibilities
                .Where(r => r.MaterialId.HasValue)
                .Select(r => r.MaterialId!.Value)
                .ToHashSet();

            return materials.Where(m => allowedIds.Contains(m.Id)).ToList();
        }

        private async Task<List<Product>> FilterProductsByResponsibilityAsync(IEnumerable<Product> products)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return new List<Product>();
            }

            if (await IsPrivilegedUserAsync())
            {
                return products.ToList();
            }

            var hasManageResponsibility = await _apiService.CheckPermissionAsync(currentUser.Id, "ManageResponsibility");
            if (hasManageResponsibility)
            {
                return products.ToList();
            }

            var responsibilities = await _apiService.GetResponsibilitiesByUserAsync(currentUser.Id, true);
            var allowedIds = responsibilities
                .Where(r => r.ProductId.HasValue)
                .Select(r => r.ProductId!.Value)
                .ToHashSet();

            return products.Where(p => allowedIds.Contains(p.Id)).ToList();
        }

        /// <summary>Для непривилегированных пользователей оставляет только партии по продуктам, за которые они ответственны.</summary>
        public async Task<List<ProductBatch>> FilterProductBatchesByResponsibilityAsync(IEnumerable<ProductBatch> batches)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return new List<ProductBatch>();
            }

            if (await IsPrivilegedUserAsync())
            {
                return batches.ToList();
            }

            var hasManageResponsibility = await _apiService.CheckPermissionAsync(currentUser.Id, "ManageResponsibility");
            if (hasManageResponsibility)
            {
                return batches.ToList();
            }

            var responsibilities = await _apiService.GetResponsibilitiesByUserAsync(currentUser.Id, true);
            var allowedProductIds = responsibilities
                .Where(r => r.ProductId.HasValue)
                .Select(r => r.ProductId!.Value)
                .ToHashSet();

            return batches.Where(b => allowedProductIds.Contains(b.ProductId)).ToList();
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
                    await LoadProductsForCatalogAsync();
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
                    await LoadProductsForCatalogAsync();
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
                var message = response.Message ?? string.Empty;
                var isError = message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                              message.Contains("ошибка", StringComparison.OrdinalIgnoreCase);

                if (!isError)
                {
                    await LoadProductsAsync();
                    await LoadProductsForCatalogAsync();
                    return true;
                }

                ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Ошибка удаления продукта" : message;
                return false;
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
                var message = response.Message ?? string.Empty;
                var isError = message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                              message.Contains("ошибка", StringComparison.OrdinalIgnoreCase);

                if (!isError)
                {
                    await LoadMaterialsAsync();
                    return true;
                }

                ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Ошибка удаления материала" : message;
                return false;
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
                if (response.IsSuccess)
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

        private async Task ApplyMaterialStockAsync(List<Material> materials)
        {
            if (materials.Count == 0)
            {
                return;
            }

            var warehouses = await EnsureWarehousesLoadedAsync();
            var warehouseMap = warehouses
                .Where(w => w.IsActive)
                .ToDictionary(w => w.Id, w => $"{w.Name} ({w.Type})");

            var fillings = await _apiService.GetAllFillingWarehousesAsync();
            var byMaterial = fillings
                .Where(f => f.MaterialId.HasValue)
                .GroupBy(f => f.MaterialId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var material in materials)
            {
                if (!byMaterial.TryGetValue(material.Id, out var materialFillings) || materialFillings.Count == 0)
                {
                    material.StockSummary = "Склад: —";
                    continue;
                }

                var parts = new List<string>();
                foreach (var filling in materialFillings)
                {
                    if (!warehouseMap.ContainsKey(filling.WarehouseId))
                    {
                        continue;
                    }
                    var warehouseLabel = warehouseMap.TryGetValue(filling.WarehouseId, out var name)
                        ? name
                        : $"Склад #{filling.WarehouseId}";
                    var unit = string.IsNullOrWhiteSpace(filling.MeasuringType)
                        ? material.MeasuringUnit
                        : filling.MeasuringType!;
                    parts.Add($"{warehouseLabel}: {filling.Quantity} {unit}");
                }

                material.StockSummary = parts.Count == 0
                    ? "Склад: —"
                    : $"Склады: {string.Join("; ", parts)}";
            }
        }

        private async Task ApplyProductStockAsync(List<Product> products)
        {
            if (products.Count == 0)
            {
                return;
            }

            var warehouses = await EnsureWarehousesLoadedAsync();
            var warehouseMap = warehouses
                .Where(w => w.IsActive)
                .ToDictionary(w => w.Id, w => $"{w.Name} ({w.Type})");

            var fillings = await _apiService.GetAllFillingWarehousesAsync();
            var byProduct = fillings
                .Where(f => f.ProductId.HasValue)
                .GroupBy(f => f.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var product in products)
            {
                if (!byProduct.TryGetValue(product.Id, out var productFillings) || productFillings.Count == 0)
                {
                    product.StockSummary = "Склад: —";
                    continue;
                }

                var parts = new List<string>();
                foreach (var filling in productFillings)
                {
                    if (!warehouseMap.ContainsKey(filling.WarehouseId))
                    {
                        continue;
                    }
                    var warehouseLabel = warehouseMap.TryGetValue(filling.WarehouseId, out var name)
                        ? name
                        : $"Склад #{filling.WarehouseId}";
                    var unit = string.IsNullOrWhiteSpace(filling.MeasuringType)
                        ? product.MeasuringUnit
                        : filling.MeasuringType!;
                    parts.Add($"{warehouseLabel}: {filling.Quantity} {unit}");
                }

                product.StockSummary = parts.Count == 0
                    ? "Склад: —"
                    : $"Склады: {string.Join("; ", parts)}";
            }
        }

        private async Task ApplyMaterialResponsibilityAsync(List<Material> materials, bool includeResponsibility)
        {
            foreach (var material in materials)
            {
                material.ResponsibilityDisplay = "Ответственный: —";
            }

            if (!includeResponsibility || materials.Count == 0)
            {
                return;
            }

            var assignments = await _apiService.GetActiveMaterialAssignmentsAsync();
            if (assignments.Count == 0)
            {
                await ApplyMaterialResponsibilityFallbackAsync(materials);
                return;
            }

            // Группируем по ItemId для поддержки нескольких пользователей
            var assignmentGroups = assignments.GroupBy(a => a.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var material in materials)
            {
                if (assignmentGroups.TryGetValue(material.Id, out var materialAssignments) && materialAssignments.Count > 0)
                {
                    var displayParts = new List<string>();
                    foreach (var assignment in materialAssignments)
                    {
                        if (assignment.Quantity.HasValue)
                        {
                            var unit = !string.IsNullOrWhiteSpace(assignment.MeasuringUnit) 
                                ? assignment.MeasuringUnit 
                                : material.MeasuringUnit;
                            displayParts.Add($"{assignment.UserName} ({assignment.Quantity} {unit})");
                        }
                        else
                        {
                            displayParts.Add(assignment.UserName);
                        }
                    }
                    material.ResponsibilityDisplay = $"Ответственные: {string.Join(", ", displayParts)}";
                }
            }
        }

        private async Task ApplyProductResponsibilityAsync(List<Product> products, bool includeResponsibility)
        {
            foreach (var product in products)
            {
                product.ResponsibilityDisplay = "Ответственный: —";
            }

            if (!includeResponsibility || products.Count == 0)
            {
                return;
            }

            var assignments = await _apiService.GetActiveProductAssignmentsAsync();
            if (assignments.Count == 0)
            {
                await ApplyProductResponsibilityFallbackAsync(products);
                return;
            }

            // Группируем по ItemId для поддержки нескольких пользователей
            var assignmentGroups = assignments.GroupBy(a => a.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var product in products)
            {
                if (assignmentGroups.TryGetValue(product.Id, out var productAssignments) && productAssignments.Count > 0)
                {
                    var displayParts = new List<string>();
                    foreach (var assignment in productAssignments)
                    {
                        if (assignment.Quantity.HasValue)
                        {
                            var unit = !string.IsNullOrWhiteSpace(assignment.MeasuringUnit) 
                                ? assignment.MeasuringUnit 
                                : product.MeasuringUnit;
                            displayParts.Add($"{assignment.UserName} ({assignment.Quantity} {unit})");
                        }
                        else
                        {
                            displayParts.Add(assignment.UserName);
                        }
                    }
                    product.ResponsibilityDisplay = $"Ответственные: {string.Join(", ", displayParts)}";
                }
            }
        }

        private async Task ApplyMaterialResponsibilityFallbackAsync(List<Material> materials)
        {
            var users = await _apiService.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id, u => $"{u.Surname} {u.Name}");

            foreach (var material in materials)
            {
                var responsibility = await _apiService.GetResponsibilityByMaterialAsync(material.Id, true)
                    ?? await _apiService.GetResponsibilityByMaterialAsync(material.Id, false);

                if (responsibility == null)
                {
                    continue;
                }

                if (!userMap.TryGetValue(responsibility.UserId, out var name))
                {
                    var user = await _apiService.GetUserByIdAsync(responsibility.UserId);
                    if (user != null)
                    {
                        name = $"{user.Surname} {user.Name}";
                        userMap[responsibility.UserId] = name;
                    }
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (responsibility.Quantity.HasValue)
                    {
                        var unit = !string.IsNullOrWhiteSpace(responsibility.MeasuringUnit) 
                            ? responsibility.MeasuringUnit 
                            : material.MeasuringUnit;
                        material.ResponsibilityDisplay = $"Ответственный: {name} ({responsibility.Quantity} {unit})";
                    }
                    else
                    {
                        material.ResponsibilityDisplay = $"Ответственный: {name}";
                    }
                }
            }
        }

        private async Task ApplyProductResponsibilityFallbackAsync(List<Product> products)
        {
            var users = await _apiService.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id, u => $"{u.Surname} {u.Name}");

            foreach (var product in products)
            {
                var responsibility = await _apiService.GetResponsibilityByProductAsync(product.Id, true)
                    ?? await _apiService.GetResponsibilityByProductAsync(product.Id, false);

                if (responsibility == null)
                {
                    continue;
                }

                if (!userMap.TryGetValue(responsibility.UserId, out var name))
                {
                    var user = await _apiService.GetUserByIdAsync(responsibility.UserId);
                    if (user != null)
                    {
                        name = $"{user.Surname} {user.Name}";
                        userMap[responsibility.UserId] = name;
                    }
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (responsibility.Quantity.HasValue)
                    {
                        var unit = !string.IsNullOrWhiteSpace(responsibility.MeasuringUnit) 
                            ? responsibility.MeasuringUnit 
                            : product.MeasuringUnit;
                        product.ResponsibilityDisplay = $"Ответственный: {name} ({responsibility.Quantity} {unit})";
                    }
                    else
                    {
                        product.ResponsibilityDisplay = $"Ответственный: {name}";
                    }
                }
            }
        }

        /// <summary>
        /// Создает список MaterialDisplayItem из списка материалов
        /// Для каждого материала с несколькими ответственными создается несколько карточек
        /// Если сумма количеств ответственных меньше общего количества на складах, добавляется карточка для неответственной части
        /// </summary>
        private async Task<List<MaterialDisplayItem>> CreateMaterialDisplayItemsAsync(List<Material> materials)
        {
            var result = new List<MaterialDisplayItem>();
            
            if (materials.Count == 0)
            {
                return result;
            }

            // Получаем все назначения ответственности
            var assignments = await _apiService.GetActiveMaterialAssignmentsAsync();
            var assignmentGroups = assignments.GroupBy(a => a.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            // Получаем информацию о количестве на складах
            var fillings = await _apiService.GetAllFillingWarehousesAsync();
            var materialTotalQuantities = fillings
                .Where(f => f.MaterialId.HasValue)
                .GroupBy(f => f.MaterialId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(f => f.Quantity));

            // Получаем информацию о складах для отображения названий
            var warehouses = await EnsureWarehousesLoadedAsync();
            var warehouseMap = warehouses
                .Where(w => w.IsActive)
                .ToDictionary(w => w.Id, w => $"{w.Name} ({w.Type})");

            // Группируем fillings по материалам
            var materialFillings = fillings
                .Where(f => f.MaterialId.HasValue)
                .GroupBy(f => f.MaterialId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var material in materials)
            {
                if (assignmentGroups.TryGetValue(material.Id, out var materialAssignments) && materialAssignments.Count > 0)
                {
                    // Получаем fillings для этого материала
                    var materialFillingList = materialFillings.TryGetValue(material.Id, out var fillingsList) 
                        ? fillingsList 
                        : new List<FillingWarehouse>();

                    // Создаем отдельную карточку для каждого ответственного (по одному на склад)
                    foreach (var assignment in materialAssignments)
                    {
                        var warehouseStocks = new List<WarehouseStockInfo>();
                        // Показываем только склад этой ответственности (из ResponsibilityFilling)
                        if (assignment.WarehouseId.HasValue)
                        {
                            var warehouseName = !string.IsNullOrWhiteSpace(assignment.WarehouseName)
                                ? assignment.WarehouseName
                                : (warehouseMap.TryGetValue(assignment.WarehouseId.Value, out var name) ? name : $"Склад #{assignment.WarehouseId}");
                            var qty = assignment.Quantity ?? 0;
                            var unit = !string.IsNullOrWhiteSpace(assignment.MeasuringUnit) ? assignment.MeasuringUnit : material.MeasuringUnit;
                            warehouseStocks.Add(new WarehouseStockInfo
                            {
                                WarehouseName = warehouseName,
                                Quantity = qty,
                                MeasuringUnit = unit ?? "ед."
                            });
                        }
                        else
                        {
                            // Старая модель без склада: показываем все склады, где есть материал (как раньше)
                            foreach (var filling in materialFillingList.Where(f => f.Quantity > 0))
                            {
                                if (warehouseMap.TryGetValue(filling.WarehouseId, out var warehouseName))
                                {
                                    var unit = !string.IsNullOrWhiteSpace(filling.MeasuringType) ? filling.MeasuringType : material.MeasuringUnit;
                                    warehouseStocks.Add(new WarehouseStockInfo
                                    {
                                        WarehouseName = warehouseName,
                                        Quantity = filling.Quantity,
                                        MeasuringUnit = unit ?? "ед."
                                    });
                                }
                            }
                        }

                        result.Add(new MaterialDisplayItem
                        {
                            Material = material,
                            Responsibility = assignment,
                            WarehouseStocks = warehouseStocks
                        });
                    }

                    // Проверяем, есть ли неответственная часть
                    var totalResponsibleQuantity = materialAssignments
                        .Where(a => a.Quantity.HasValue)
                        .Sum(a => a.Quantity!.Value);

                    // Если есть ответственные с количеством, проверяем общее количество на складах
                    if (materialAssignments.Any(a => a.Quantity.HasValue) && 
                        materialTotalQuantities.TryGetValue(material.Id, out var totalOnWarehouses))
                    {
                        // Если сумма количеств ответственных меньше общего количества на складах
                        if (totalResponsibleQuantity < totalOnWarehouses)
                        {
                            var unassignedQty = totalOnWarehouses - totalResponsibleQuantity;
                            result.Add(new MaterialDisplayItem
                            {
                                Material = material,
                                Responsibility = null, // Неответственная часть
                                UnassignedQuantity = unassignedQty,
                                UnassignedMeasuringUnit = material.MeasuringUnit
                            });
                        }
                    }
                    // Если есть ответственные без количества (за весь материал), не добавляем неответственную часть
                }
                else
                {
                    // Если нет ответственных, создаем одну карточку без ответственности
                    var totalOnWarehouses = materialTotalQuantities.GetValueOrDefault(material.Id, 0);
                    result.Add(new MaterialDisplayItem
                    {
                        Material = material,
                        Responsibility = null,
                        UnassignedQuantity = totalOnWarehouses > 0 ? totalOnWarehouses : null,
                        UnassignedMeasuringUnit = material.MeasuringUnit
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Создает список ProductDisplayItem из списка продуктов
        /// Для каждого продукта с несколькими ответственными создается несколько карточек
        /// Если сумма количеств ответственных меньше общего количества на складах, добавляется карточка для неответственной части
        /// </summary>
        private async Task<List<ProductDisplayItem>> CreateProductDisplayItemsAsync(List<Product> products)
        {
            var result = new List<ProductDisplayItem>();
            
            if (products.Count == 0)
            {
                return result;
            }

            // Получаем все назначения ответственности
            var assignments = await _apiService.GetActiveProductAssignmentsAsync();
            var assignmentGroups = assignments.GroupBy(a => a.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            // Получаем информацию о количестве на складах
            var fillings = await _apiService.GetAllFillingWarehousesAsync();
            var productTotalQuantities = fillings
                .Where(f => f.ProductId.HasValue)
                .GroupBy(f => f.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(f => f.Quantity));

            // Получаем информацию о складах для отображения названий
            var warehouses = await EnsureWarehousesLoadedAsync();
            var warehouseMap = warehouses
                .Where(w => w.IsActive)
                .ToDictionary(w => w.Id, w => $"{w.Name} ({w.Type})");

            // Группируем fillings по продуктам
            var productFillings = fillings
                .Where(f => f.ProductId.HasValue)
                .GroupBy(f => f.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var product in products)
            {
                if (assignmentGroups.TryGetValue(product.Id, out var productAssignments) && productAssignments.Count > 0)
                {
                    // Получаем fillings для этого продукта
                    var productFillingList = productFillings.TryGetValue(product.Id, out var fillingsList) 
                        ? fillingsList 
                        : new List<FillingWarehouse>();

                    // Создаем отдельную карточку для каждого ответственного (по одному на склад)
                    foreach (var assignment in productAssignments)
                    {
                        var warehouseStocks = new List<WarehouseStockInfo>();
                        if (assignment.WarehouseId.HasValue)
                        {
                            var warehouseName = !string.IsNullOrWhiteSpace(assignment.WarehouseName)
                                ? assignment.WarehouseName
                                : (warehouseMap.TryGetValue(assignment.WarehouseId.Value, out var name) ? name : $"Склад #{assignment.WarehouseId}");
                            var qty = assignment.Quantity ?? 0;
                            var unit = !string.IsNullOrWhiteSpace(assignment.MeasuringUnit) ? assignment.MeasuringUnit : product.MeasuringUnit;
                            warehouseStocks.Add(new WarehouseStockInfo
                            {
                                WarehouseName = warehouseName,
                                Quantity = qty,
                                MeasuringUnit = unit ?? "ед."
                            });
                        }
                        else
                        {
                            foreach (var filling in productFillingList.Where(f => f.Quantity > 0))
                            {
                                if (warehouseMap.TryGetValue(filling.WarehouseId, out var warehouseName))
                                {
                                    var unit = !string.IsNullOrWhiteSpace(filling.MeasuringType) ? filling.MeasuringType : product.MeasuringUnit;
                                    warehouseStocks.Add(new WarehouseStockInfo
                                    {
                                        WarehouseName = warehouseName,
                                        Quantity = filling.Quantity,
                                        MeasuringUnit = unit ?? "ед."
                                    });
                                }
                            }
                        }

                        result.Add(new ProductDisplayItem
                        {
                            Product = product,
                            Responsibility = assignment,
                            WarehouseStocks = warehouseStocks
                        });
                    }

                    // Проверяем, есть ли неответственная часть
                    var totalResponsibleQuantity = productAssignments
                        .Where(a => a.Quantity.HasValue)
                        .Sum(a => a.Quantity!.Value);

                    // Если есть ответственные с количеством, проверяем общее количество на складах
                    if (productAssignments.Any(a => a.Quantity.HasValue) && 
                        productTotalQuantities.TryGetValue(product.Id, out var totalOnWarehouses))
                    {
                        // Если сумма количеств ответственных меньше общего количества на складах
                        if (totalResponsibleQuantity < totalOnWarehouses)
                        {
                            var unassignedQty = totalOnWarehouses - totalResponsibleQuantity;
                            result.Add(new ProductDisplayItem
                            {
                                Product = product,
                                Responsibility = null, // Неответственная часть
                                UnassignedQuantity = unassignedQty,
                                UnassignedMeasuringUnit = product.MeasuringUnit
                            });
                        }
                    }
                    // Если есть ответственные без количества (за весь продукт), не добавляем неответственную часть
                }
                else
                {
                    // Если нет ответственных, создаем одну карточку без ответственности
                    var totalOnWarehouses = productTotalQuantities.GetValueOrDefault(product.Id, 0);
                    result.Add(new ProductDisplayItem
                    {
                        Product = product,
                        Responsibility = null,
                        UnassignedQuantity = totalOnWarehouses > 0 ? totalOnWarehouses : null,
                        UnassignedMeasuringUnit = product.MeasuringUnit
                    });
                }
            }

            return result;
        }

        private async Task<List<Warehouse>> EnsureWarehousesLoadedAsync()
        {
            if (Warehouses.Count > 0)
            {
                return Warehouses.ToList();
            }

            var warehouses = await _apiService.GetAllWarehousesAsync();
            Warehouses.Clear();
            foreach (var warehouse in warehouses)
            {
                Warehouses.Add(warehouse);
            }

            return warehouses;
        }

        public async Task<bool> IsPrivilegedUserAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            if (currentUser.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (await _authService.IsAdminAsync(currentUser.Id))
            {
                return true;
            }

            var (roleCode, roleName) = await GetRoleInfoAsync(currentUser);
            return roleCode.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                   roleCode.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   roleCode.Equals("SeniorExtruder", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Equals("Владелец", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Equals("Администратор", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Equals("Старший экструзионщик", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> HasManageResponsibilityAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            return await _apiService.CheckPermissionAsync(currentUser.Id, "ManageResponsibility");
        }

        private async Task<(string Code, string Name)> GetRoleInfoAsync(User user)
        {
            if (user.Role != null)
            {
                return (user.Role.Code ?? string.Empty, user.Role.Name ?? string.Empty);
            }

            if (user.RoleId.HasValue)
            {
                var role = await _apiService.GetRoleByIdAsync(user.RoleId.Value);
                if (role != null)
                {
                    return (role.Code ?? string.Empty, role.Name ?? string.Empty);
                }
            }

            var fullUser = await _apiService.GetUserByIdAsync(user.Id);
            if (fullUser?.Role != null)
            {
                return (fullUser.Role.Code ?? string.Empty, fullUser.Role.Name ?? string.Empty);
            }

            if (fullUser?.RoleId.HasValue == true)
            {
                var role = await _apiService.GetRoleByIdAsync(fullUser.RoleId.Value);
                if (role != null)
                {
                    return (role.Code ?? string.Empty, role.Name ?? string.Empty);
                }
            }

            return (string.Empty, string.Empty);
        }

        /// <summary>
        /// Проверяет, может ли текущий пользователь удалять продукты/материалы.
        /// Удаление разрешено привилегированным пользователям или с правом WriteOff.
        /// </summary>
        public async Task<bool> CanDeleteItemsAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            // Проверяем право WriteOff
            return await _apiService.CheckPermissionAsync(currentUser.Id, "WriteOff");
        }

        /// <summary>
        /// Проверяет право на сканирование/назначение штрих-кодов
        /// </summary>
        public async Task<bool> HasAssignBarcodePermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "AssignBarcode");
        }

        /// <summary>
        /// Проверяет право на приемку товаров (добавление продуктов/материалов)
        /// </summary>
        public async Task<bool> HasReceiveGoodsPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "ReceiveGoods");
        }

        /// <summary>
        /// Проверяет право на управление рецептурами (переработка, редактирование)
        /// </summary>
        public async Task<bool> HasManageRecipesPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "ManageRecipes");
        }

        /// <summary>
        /// Проверяет право на передачу смены
        /// </summary>
        public async Task<bool> HasShiftTransferPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "ShiftTransfer");
        }

        /// <summary>
        /// Проверяет право на отправку в утиль
        /// </summary>
        public async Task<bool> HasSendToScrapPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "SendToScrap");
        }

        /// <summary>
        /// Проверяет право на списание
        /// </summary>
        public async Task<bool> HasWriteOffPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "WriteOff");
        }

        /// <summary>
        /// Проверяет право на управление пользователями
        /// </summary>
        public async Task<bool> HasManageUsersPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;
            return await _apiService.CheckPermissionAsync(currentUser.Id, "ManageUsers");
        }

        /// <summary>
        /// Проверяет право на перенос ТМЦ (изменение количества на складах)
        /// </summary>
        public async Task<bool> HasTransferPermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;

            var hasMainToWorkshops = await _apiService.CheckPermissionAsync(currentUser.Id, "TransferMainToWorkshops");
            var hasWorkshopsToMain = await _apiService.CheckPermissionAsync(currentUser.Id, "TransferWorkshopsToMain");
            return hasMainToWorkshops || hasWorkshopsToMain;
        }

        /// <summary>
        /// Проверяет право на доступ к утилю (SendToScrap или WriteOff)
        /// </summary>
        public async Task<bool> HasDisposalAccessAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;

            var hasSendToScrap = await _apiService.CheckPermissionAsync(currentUser.Id, "SendToScrap");
            var hasWriteOff = await _apiService.CheckPermissionAsync(currentUser.Id, "WriteOff");
            return hasSendToScrap || hasWriteOff;
        }

        /// <summary>
        /// Проверяет право на добавление/редактирование (ReceiveGoods или ManageRecipes)
        /// </summary>
        public async Task<bool> CanAddOrEditItemsAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;
            if (await IsPrivilegedUserAsync()) return true;

            var hasReceiveGoods = await _apiService.CheckPermissionAsync(currentUser.Id, "ReceiveGoods");
            var hasManageRecipes = await _apiService.CheckPermissionAsync(currentUser.Id, "ManageRecipes");
            return hasReceiveGoods || hasManageRecipes;
        }

        // ==================== Product Batches ====================

        public async Task LoadProductBatchesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var batches = await _apiService.GetAllProductBatchesAsync();
                ProductBatches.Clear();
                foreach (var batch in batches)
                {
                    ProductBatches.Add(batch);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки партий: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadProductBatchesByProductAsync(int productId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var batches = await _apiService.GetProductBatchesByProductAsync(productId);
                ProductBatches.Clear();
                foreach (var batch in batches)
                {
                    ProductBatches.Add(batch);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки партий: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadProductBatchesByWarehouseAsync(int warehouseId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var batches = await _apiService.GetProductBatchesByWarehouseAsync(warehouseId);
                ProductBatches.Clear();
                foreach (var batch in batches)
                {
                    ProductBatches.Add(batch);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки партий: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadProductBatchesByUserAsync(int userId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var batches = await _apiService.GetProductBatchesByUserAsync(userId);
                ProductBatches.Clear();
                foreach (var batch in batches)
                {
                    ProductBatches.Add(batch);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки партий: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> CreateProductBatchAsync(ProductBatch batch)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var response = await _apiService.CreateProductBatchAsync(batch);
                if (response.Batch != null)
                {
                    await LoadProductBatchesAsync();
                    return true;
                }
                ErrorMessage = response.Message ?? "Не удалось создать партию";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания партии: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> UpdateProductBatchAsync(int id, ProductBatch batch)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var response = await _apiService.UpdateProductBatchAsync(id, batch);
                if (response.Batch != null)
                {
                    await LoadProductBatchesAsync();
                    return true;
                }
                ErrorMessage = response.Message ?? "Не удалось обновить партию";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка обновления партии: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> DeleteProductBatchAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var response = await _apiService.DeleteProductBatchAsync(id);
                if (response.Message?.Contains("успешно") == true || response.Message?.Contains("deleted") == true)
                {
                    await LoadProductBatchesAsync();
                    return true;
                }
                ErrorMessage = response.Message ?? "Не удалось удалить партию";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления партии: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class DisposalItem : INotifyPropertyChanged
    {
        public int FillingId { get; set; }
        public int WarehouseId { get; set; }
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string MeasuringUnit { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // "Материал" или "Продукт"
        public string ItemIcon => ItemType == "Продукт" ? "📦" : "🔧";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class DisposalViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private Warehouse? _disposalWarehouse;
        private bool _canDelete;

        public ObservableCollection<DisposalItem> Items { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        public bool CanDelete
        {
            get => _canDelete;
            set { _canDelete = value; OnPropertyChanged(); }
        }

        public Warehouse? DisposalWarehouse => _disposalWarehouse;

        public DisposalViewModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Проверяем права на удаление
                await CheckDeletePermissionAsync();

                // Находим склад "Утиль"
                var warehouses = await _apiService.GetAllWarehousesAsync();
                _disposalWarehouse = warehouses.FirstOrDefault(w =>
                    w.Type.Equals("Утиль", StringComparison.OrdinalIgnoreCase) ||
                    w.Name.Contains("Утиль", StringComparison.OrdinalIgnoreCase));

                if (_disposalWarehouse == null)
                {
                    ErrorMessage = "Склад утиля не найден. Создайте склад с типом 'Утиль'.";
                    return;
                }

                // Загружаем справочники
                var materialsTask = _apiService.GetAllMaterialsAsync();
                var productsTask = _apiService.GetAllProductsAsync();
                var fillingsTask = _apiService.GetFillingsByWarehouseAsync(_disposalWarehouse.Id);

                await Task.WhenAll(materialsTask, productsTask, fillingsTask);

                var materials = materialsTask.Result ?? new List<Material>();
                var products = productsTask.Result ?? new List<Product>();
                var fillings = fillingsTask.Result ?? new List<FillingWarehouse>();

                var materialMap = materials.ToDictionary(m => m.Id, m => m);
                var productMap = products.ToDictionary(p => p.Id, p => p);

                Items.Clear();

                foreach (var filling in fillings.Where(f => f.Quantity > 0))
                {
                    var item = new DisposalItem
                    {
                        FillingId = filling.Id,
                        WarehouseId = filling.WarehouseId,
                        MaterialId = filling.MaterialId,
                        ProductId = filling.ProductId,
                        Quantity = filling.Quantity,
                        MeasuringUnit = filling.MeasuringType ?? "шт"
                    };

                    if (filling.MaterialId.HasValue && materialMap.TryGetValue(filling.MaterialId.Value, out var material))
                    {
                        item.Name = material.Name;
                        item.Code = material.Code;
                        item.ItemType = "Материал";
                        item.MeasuringUnit = material.MeasuringUnit;
                    }
                    else if (filling.ProductId.HasValue && productMap.TryGetValue(filling.ProductId.Value, out var product))
                    {
                        item.Name = product.Name;
                        item.Code = product.Code;
                        item.ItemType = "Продукт";
                        item.MeasuringUnit = product.MeasuringUnit;
                    }
                    else
                    {
                        item.Name = "Неизвестный элемент";
                        item.ItemType = filling.MaterialId.HasValue ? "Материал" : "Продукт";
                    }

                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CheckDeletePermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                CanDelete = false;
                return;
            }

            // Проверяем роль Владельца или Администратора
            var roleCode = currentUser.Role?.Code ?? string.Empty;
            var roleName = currentUser.Role?.Name ?? string.Empty;
            if (roleCode.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                roleCode.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("Владелец", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("Администратор", StringComparison.OrdinalIgnoreCase) ||
                currentUser.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                CanDelete = true;
                return;
            }

            // Проверяем права роли
            if (currentUser.RoleId.HasValue)
            {
                var rolePermissions = await _apiService.GetRolePermissionsAsync(currentUser.RoleId.Value);
                if (rolePermissions.Any(p => p.Code == "WriteOff" || p.Code == "SendToScrap"))
                {
                    CanDelete = true;
                    return;
                }
            }

            // Проверяем персональные права
            var userPermissions = await _apiService.GetUserPermissionsAsync(currentUser.Id);
            CanDelete = userPermissions.Any(p => p.Code == "WriteOff" || p.Code == "SendToScrap");
        }

        public async Task<bool> DeleteItemAsync(DisposalItem item, string reason)
        {
            if (_disposalWarehouse == null)
                return false;

            try
            {
                ApiResponse<object> result;
                if (item.MaterialId.HasValue)
                {
                    result = await _apiService.DeleteFillingWarehouseByMaterialAsync(
                        _disposalWarehouse.Id, item.MaterialId.Value);
                }
                else if (item.ProductId.HasValue)
                {
                    result = await _apiService.DeleteFillingWarehouseByProductAsync(
                        _disposalWarehouse.Id, item.ProductId.Value);
                }
                else
                {
                    return false;
                }

                if (result.IsSuccess)
                {
                    Items.Remove(item);
                    return true;
                }

                ErrorMessage = result.Message ?? "Не удалось удалить элемент";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления: {ex.Message}";
                return false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

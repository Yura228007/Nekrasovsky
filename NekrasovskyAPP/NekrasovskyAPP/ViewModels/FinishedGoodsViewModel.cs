using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;
using Microsoft.Maui.Controls;

namespace NekrasovskyAPP.ViewModels
{
    public class FinishedGoodsViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private Warehouse? _finishedGoodsWarehouse;
        private bool _canManage;

        public ObservableCollection<FinishedGoodsRequest> Requests { get; } = new();
        public ObservableCollection<FinishedGoodsItem> Items { get; } = new();

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

        public bool CanManage
        {
            get => _canManage;
            set { _canManage = value; OnPropertyChanged(); }
        }

        public Warehouse? FinishedGoodsWarehouse => _finishedGoodsWarehouse;

        public FinishedGoodsViewModel(IApiService apiService, IAuthService authService)
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

                // Проверяем права на управление
                await CheckManagePermissionAsync();

                // Загружаем справочники
                var productsTask = _apiService.GetAllProductsAsync();
                var warehousesTask = _apiService.GetAllWarehousesAsync();
                var usersTask = _apiService.GetAllUsersAsync();

                await Task.WhenAll(productsTask, warehousesTask, usersTask);

                var products = productsTask.Result ?? new List<Product>();
                var warehouses = warehousesTask.Result ?? new List<Warehouse>();
                var users = usersTask.Result ?? new List<User>();

                // Находим склад готовой продукции (первый найденный)
                _finishedGoodsWarehouse = warehouses.FirstOrDefault(w =>
                    w.IsActive && (w.Type?.Contains("Готовая продукция", StringComparison.OrdinalIgnoreCase) == true ||
                                   w.Name.Contains("Готовая продукция", StringComparison.OrdinalIgnoreCase)));

                if (_finishedGoodsWarehouse == null)
                {
                    ErrorMessage = "Склад готовой продукции не найден. Создайте склад с типом 'Готовая продукция'.";
                    return;
                }

                var fillingsTask = _apiService.GetFillingsByWarehouseAsync(_finishedGoodsWarehouse.Id);
                var requestsTask = _apiService.GetPendingFinishedGoodsRequestsAsync(_finishedGoodsWarehouse.Id);

                await Task.WhenAll(fillingsTask, requestsTask);

                var fillings = fillingsTask.Result ?? new List<FillingWarehouse>();
                var requests = requestsTask.Result ?? new List<FinishedGoodsRequest>();

                var productMap = products.ToDictionary(p => p.Id, p => p);
                var warehouseMap = warehouses.ToDictionary(w => w.Id, w => w);
                var userMap = users.ToDictionary(u => u.Id, u => $"{u.Surname} {u.Name}");

                Items.Clear();
                Requests.Clear();

                // Заполняем наполнение склада
                foreach (var filling in fillings.Where(f => f.Quantity > 0 && f.ProductId.HasValue))
                {
                    var item = new FinishedGoodsItem
                    {
                        FillingId = filling.Id,
                        WarehouseId = filling.WarehouseId,
                        ProductId = filling.ProductId!.Value,
                        Quantity = filling.Quantity,
                        MeasuringUnit = filling.MeasuringType ?? "шт"
                    };

                    if (productMap.TryGetValue(filling.ProductId.Value, out var product))
                    {
                        item.Product = product;
                        item.MeasuringUnit = product.MeasuringUnit;
                    }

                    Items.Add(item);
                }

                // Заполняем запросы
                foreach (var request in requests)
                {
                    if (productMap.TryGetValue(request.ProductId, out var product))
                    {
                        // Product уже есть в навигационном свойстве
                    }
                    if (warehouseMap.TryGetValue(request.FromWarehouseId, out var fromWarehouse))
                    {
                        request.FromWarehouse = fromWarehouse;
                    }
                    if (warehouseMap.TryGetValue(request.ToWarehouseId, out var toWarehouse))
                    {
                        request.ToWarehouse = toWarehouse;
                    }
                    if (userMap.TryGetValue(request.FromUserId, out var fromUserName))
                    {
                        if (request.FromUser == null)
                        {
                            request.FromUser = new User { Id = request.FromUserId, Name = fromUserName.Split(' ').LastOrDefault() ?? "", Surname = fromUserName.Split(' ').FirstOrDefault() ?? "" };
                        }
                    }
                    Requests.Add(request);
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

        private async Task CheckManagePermissionAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                CanManage = false;
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
                CanManage = true;
                return;
            }

            // Проверяем права роли
            if (currentUser.RoleId.HasValue)
            {
                var rolePermissions = await _apiService.GetRolePermissionsAsync(currentUser.RoleId.Value);
                if (rolePermissions.Any(p => p.Code == "ManageFinishedGoodsWarehouses"))
                {
                    CanManage = true;
                    return;
                }
            }

            // Проверяем персональные права
            var userPermissions = await _apiService.GetUserPermissionsAsync(currentUser.Id);
            CanManage = userPermissions.Any(p => p.Code == "ManageFinishedGoodsWarehouses");
        }

        public async Task<bool> ApproveFinishedGoodsRequestAsync(FinishedGoodsRequest request)
        {
            try
            {
                var result = await _apiService.ApproveFinishedGoodsRequestAsync(request.Id);
                if (result.IsSuccess)
                {
                    await LoadDataAsync();
                    return true;
                }
                ErrorMessage = result.Message ?? "Не удалось подтвердить запрос";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> RejectFinishedGoodsRequestAsync(FinishedGoodsRequest request)
        {
            try
            {
                var result = await _apiService.RejectFinishedGoodsRequestAsync(request.Id);
                if (result.IsSuccess)
                {
                    await LoadDataAsync();
                    return true;
                }
                ErrorMessage = result.Message ?? "Не удалось отклонить запрос";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> ProcessFinishedGoodsAsync(FinishedGoodsItem item)
        {
            if (_finishedGoodsWarehouse == null)
                return false;

            var saleQty = double.TryParse(item.SaleQty, out var s) ? s : 0;
            var disposalQty = double.TryParse(item.DisposalQty, out var d) ? d : 0;

            if (saleQty <= 0 && disposalQty <= 0)
            {
                ErrorMessage = "Укажите количество для продажи и/или отправки в утиль.";
                return false;
            }

            if (saleQty + disposalQty > item.Quantity)
            {
                ErrorMessage = $"Сумма количеств не может превышать остаток ({item.Quantity} {item.MeasuringUnit}).";
                return false;
            }

            item.IsProcessing = true;
            ErrorMessage = string.Empty;

            try
            {
                // Обрабатываем продажу
                if (saleQty > 0)
                {
                    var saleResult = await _apiService.ProcessProductSaleAsync(
                        _finishedGoodsWarehouse.Id,
                        item.ProductId,
                        saleQty,
                        item.MeasuringUnit);

                    if (!saleResult.IsSuccess)
                    {
                        ErrorMessage = saleResult.Message ?? "Не удалось оформить продажу";
                        return false;
                    }
                }

                // Обрабатываем утиль
                if (disposalQty > 0)
                {
                    var disposalResult = await _apiService.ProcessFinishedGoodsDisposalAsync(
                        _finishedGoodsWarehouse.Id,
                        item.ProductId,
                        disposalQty,
                        item.MeasuringUnit);

                    if (!disposalResult.IsSuccess)
                    {
                        ErrorMessage = disposalResult.Message ?? "Не удалось отправить в утиль";
                        return false;
                    }
                }

                await LoadDataAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                return false;
            }
            finally
            {
                item.IsProcessing = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

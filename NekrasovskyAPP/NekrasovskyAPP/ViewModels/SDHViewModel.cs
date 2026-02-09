using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class SDHViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private Warehouse? _sdhWarehouse;
        private bool _canManage;

        public ObservableCollection<SDHRequest> Requests { get; } = new();
        public ObservableCollection<SDHItem> Items { get; } = new();

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

        public Warehouse? SDHWarehouse => _sdhWarehouse;

        public SDHViewModel(IApiService apiService, IAuthService authService)
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
                var materialsTask = _apiService.GetAllMaterialsAsync();
                var productsTask = _apiService.GetAllProductsAsync();
                var warehousesTask = _apiService.GetAllWarehousesAsync();
                var usersTask = _apiService.GetAllUsersAsync();

                await Task.WhenAll(materialsTask, productsTask, warehousesTask, usersTask);

                var materials = materialsTask.Result ?? new List<Material>();
                var products = productsTask.Result ?? new List<Product>();
                var warehouses = warehousesTask.Result ?? new List<Warehouse>();
                var users = usersTask.Result ?? new List<User>();

                // Находим склад СДХ
                _sdhWarehouse = warehouses.FirstOrDefault(w =>
                    w.IsActive && (w.Type?.Equals("СДХ", StringComparison.OrdinalIgnoreCase) == true ||
                                   w.Name.Contains("СДХ", StringComparison.OrdinalIgnoreCase)));

                if (_sdhWarehouse == null)
                {
                    ErrorMessage = "Склад СДХ не найден. Создайте склад с типом 'СДХ'.";
                    return;
                }

                var fillingsTask = _apiService.GetFillingsByWarehouseAsync(_sdhWarehouse.Id);
                var requestsTask = _apiService.GetPendingSDHRequestsAsync(_sdhWarehouse.Id);

                await Task.WhenAll(fillingsTask, requestsTask);

                var fillings = fillingsTask.Result ?? new List<FillingWarehouse>();
                var requests = requestsTask.Result ?? new List<SDHRequest>();

                var materialMap = materials.ToDictionary(m => m.Id, m => m);
                var productMap = products.ToDictionary(p => p.Id, p => p);
                var warehouseMap = warehouses.ToDictionary(w => w.Id, w => w);
                var userMap = users.ToDictionary(u => u.Id, u => $"{u.Surname} {u.Name}");

                Items.Clear();
                Requests.Clear();

                // Заполняем наполнение склада
                foreach (var filling in fillings.Where(f => f.Quantity > 0))
                {
                    var item = new SDHItem
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
                        item.Material = material;
                        item.MeasuringUnit = material.MeasuringUnit ?? "шт";
                    }
                    else if (filling.ProductId.HasValue && productMap.TryGetValue(filling.ProductId.Value, out var product))
                    {
                        item.Product = product;
                        item.MeasuringUnit = product.MeasuringUnit ?? "шт";
                    }

                    Items.Add(item);
                }

                // Заполняем запросы
                foreach (var request in requests)
                {
                    if (request.MaterialId.HasValue && materialMap.TryGetValue(request.MaterialId.Value, out var material))
                    {
                        request.MaterialName = material.Name;
                    }
                    if (request.ProductId.HasValue && productMap.TryGetValue(request.ProductId.Value, out var product))
                    {
                        request.ProductName = product.Name;
                    }
                    if (warehouseMap.TryGetValue(request.FromWarehouseId, out var fromWarehouse))
                    {
                        request.FromWarehouseName = fromWarehouse.Name;
                    }
                    if (warehouseMap.TryGetValue(request.ToWarehouseId, out var toWarehouse))
                    {
                        request.ToWarehouseName = toWarehouse.Name;
                    }
                    if (userMap.TryGetValue(request.FromUserId, out var fromUserName))
                    {
                        request.FromUserName = fromUserName;
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
                if (rolePermissions.Any(p => p.Code == "ManageSDH"))
                {
                    CanManage = true;
                    return;
                }
            }

            // Проверяем персональные права
            var userPermissions = await _apiService.GetUserPermissionsAsync(currentUser.Id);
            CanManage = userPermissions.Any(p => p.Code == "ManageSDH");
        }

        public async Task<bool> ApproveSDHRequestAsync(SDHRequest request)
        {
            try
            {
                var result = await _apiService.ApproveSDHRequestAsync(request.Id);
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

        public async Task<bool> RejectSDHRequestAsync(SDHRequest request)
        {
            try
            {
                var result = await _apiService.RejectSDHRequestAsync(request.Id);
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

        public async Task<bool> ProcessSDHItemAsync(SDHItem item, int? transferToWarehouseId, int? transferToUserId)
        {
            if (_sdhWarehouse == null)
                return false;

            var nonReturnableDefectQty = double.TryParse(item.NonReturnableDefectQty, out var nrd) ? nrd : 0;
            var saleQty = double.TryParse(item.SaleQty, out var s) ? s : 0;
            var transferQty = double.TryParse(item.TransferQty, out var t) ? t : 0;

            if (nonReturnableDefectQty <= 0 && saleQty <= 0 && transferQty <= 0)
            {
                ErrorMessage = "Укажите количество для обработки.";
                return false;
            }

            if (nonReturnableDefectQty + saleQty + transferQty > item.Quantity)
            {
                ErrorMessage = $"Сумма количеств не может превышать остаток ({item.Quantity} {item.MeasuringUnit}).";
                return false;
            }

            if (transferQty > 0 && (!transferToWarehouseId.HasValue || !transferToUserId.HasValue))
            {
                ErrorMessage = "Для перемещения необходимо указать склад и получателя.";
                return false;
            }

            item.IsProcessing = true;
            ErrorMessage = string.Empty;

            try
            {
                // Обрабатываем невозвратный брак (списывается сразу)
                if (nonReturnableDefectQty > 0)
                {
                    var defectResult = await _apiService.ProcessSDHNonReturnableDefectAsync(
                        _sdhWarehouse.Id,
                        item.MaterialId,
                        item.ProductId,
                        nonReturnableDefectQty,
                        item.MeasuringUnit);

                    if (!defectResult.IsSuccess)
                    {
                        ErrorMessage = defectResult.Message ?? "Не удалось списать невозвратный брак";
                        return false;
                    }
                }

                // Обрабатываем продажу
                if (saleQty > 0)
                {
                    var saleResult = await _apiService.ProcessSDHSaleAsync(
                        _sdhWarehouse.Id,
                        item.MaterialId,
                        item.ProductId,
                        saleQty,
                        item.MeasuringUnit);

                    if (!saleResult.IsSuccess)
                    {
                        ErrorMessage = saleResult.Message ?? "Не удалось оформить продажу";
                        return false;
                    }
                }

                // Обрабатываем перемещение
                if (transferQty > 0 && transferToWarehouseId.HasValue && transferToUserId.HasValue)
                {
                    var currentUser = _authService.CurrentUser;
                    if (currentUser == null)
                    {
                        ErrorMessage = "Пользователь не авторизован";
                        return false;
                    }

                    var partRequest = new PartRequest
                    {
                        FromWarehouseId = _sdhWarehouse.Id,
                        ToWarehouseId = transferToWarehouseId.Value,
                        FromUserId = currentUser.Id,
                        ToUserId = transferToUserId.Value,
                        MaterialId = item.MaterialId,
                        ProductId = item.ProductId,
                        Quantity = transferQty,
                        MeasuringType = item.MeasuringUnit
                    };
                    var transferResult = await _apiService.AddPartRequestAsync(partRequest);

                    if (!transferResult.IsSuccess)
                    {
                        ErrorMessage = transferResult.Message ?? "Не удалось создать запрос на перемещение";
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

        public async Task<List<Warehouse>> GetWarehousesAsync()
        {
            try
            {
                return await _apiService.GetAllWarehousesAsync();
            }
            catch
            {
                return new List<Warehouse>();
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                return await _apiService.GetAllUsersAsync();
            }
            catch
            {
                return new List<User>();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

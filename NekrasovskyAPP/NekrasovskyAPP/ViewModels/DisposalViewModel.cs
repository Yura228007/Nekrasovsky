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
        public double Quantity { get; set; }
        public string MeasuringUnit { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // "Материал" или "Продукт"
        public string ItemIcon => ItemType == "Продукт" ? "📦" : "🔧";

        private string _returnableQty = string.Empty;
        private string _nonReturnableQty = string.Empty;
        private bool _isProcessing;

        public string ReturnableQty
        {
            get => _returnableQty;
            set { _returnableQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
        }

        public string NonReturnableQty
        {
            get => _nonReturnableQty;
            set { _nonReturnableQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set { _isProcessing = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
        }

        public bool CanProcess => !IsProcessing && (HasReturnable || HasNonReturnable);

        public bool HasReturnable => int.TryParse(_returnableQty, out var v) && v > 0;
        public bool HasNonReturnable => int.TryParse(_nonReturnableQty, out var v) && v > 0;

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
        public ObservableCollection<DisposalRequest> Requests { get; } = new();

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

                // Загружаем справочники, наполнение и запросы
                var materialsTask = _apiService.GetAllMaterialsAsync();
                var productsTask = _apiService.GetAllProductsAsync();
                var warehousesTask = _apiService.GetAllWarehousesAsync();
                var usersTask = _apiService.GetAllUsersAsync();

                await Task.WhenAll(materialsTask, productsTask, warehousesTask, usersTask);

                var materials = materialsTask.Result ?? new List<Material>();
                var products = productsTask.Result ?? new List<Product>();
                var warehouses = warehousesTask.Result ?? new List<Warehouse>();
                var users = usersTask.Result ?? new List<User>();

                // Находим склад "Утиль"
                _disposalWarehouse = warehouses.FirstOrDefault(w =>
                    w.Type.Equals("Утиль", StringComparison.OrdinalIgnoreCase) ||
                    w.Name.Contains("Утиль", StringComparison.OrdinalIgnoreCase));

                if (_disposalWarehouse == null)
                {
                    ErrorMessage = "Склад утиля не найден. Создайте склад с типом 'Утиль'.";
                    return;
                }

                var fillingsTask = _apiService.GetFillingsByWarehouseAsync(_disposalWarehouse.Id);
                var requestsTask = _apiService.GetPendingDisposalRequestsAsync(_disposalWarehouse.Id);

                await Task.WhenAll(fillingsTask, requestsTask);

                var fillings = fillingsTask.Result ?? new List<FillingWarehouse>();
                var requests = requestsTask.Result ?? new List<DisposalRequest>();

                var materialMap = materials.ToDictionary(m => m.Id, m => m);
                var productMap = products.ToDictionary(p => p.Id, p => p);
                var warehouseMap = warehouses.ToDictionary(w => w.Id, w => w);
                var userMap = users.ToDictionary(u => u.Id, u => $"{u.Surname} {u.Name}");

                Items.Clear();
                Requests.Clear();

                // Заполняем наполнение склада
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
                        item.Code = material.Code ?? string.Empty;
                        item.ItemType = "Материал";
                        item.MeasuringUnit = material.MeasuringUnit;
                    }
                    else if (filling.ProductId.HasValue && productMap.TryGetValue(filling.ProductId.Value, out var product))
                    {
                        item.Name = product.Name;
                        item.Code = product.Code ?? string.Empty;
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

                // Заполняем запросы на утиль
                // Имена уже должны быть заполнены с сервера, но заполняем на случай если их нет
                foreach (var request in requests)
                {
                    // Если имена не заполнены с сервера, заполняем на клиенте
                    if (string.IsNullOrEmpty(request.MaterialName) && string.IsNullOrEmpty(request.ProductName))
                    {
                        if (request.MaterialId.HasValue && materialMap.TryGetValue(request.MaterialId.Value, out var material))
                        {
                            request.MaterialName = material.Name;
                        }
                        else if (request.ProductId.HasValue && productMap.TryGetValue(request.ProductId.Value, out var product))
                        {
                            request.ProductName = product.Name;
                            request.MaterialName = product.Name; // Для отображения используем MaterialName
                        }
                    }
                    
                    if (string.IsNullOrEmpty(request.FromWarehouseName) && warehouseMap.TryGetValue(request.FromWarehouseId, out var fromWarehouse))
                    {
                        request.FromWarehouseName = fromWarehouse.Name;
                    }
                    if (string.IsNullOrEmpty(request.ToWarehouseName) && warehouseMap.TryGetValue(request.ToWarehouseId, out var toWarehouse))
                    {
                        request.ToWarehouseName = toWarehouse.Name;
                    }
                    if (string.IsNullOrEmpty(request.FromUserName) && userMap.TryGetValue(request.FromUserId, out var fromUserName))
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

        public async Task<bool> ProcessDisposalAsync(DisposalItem item)
        {
            if (_disposalWarehouse == null)
                return false;

            var returnableQty = int.TryParse(item.ReturnableQty, out var r) ? r : 0;
            var nonReturnableQty = int.TryParse(item.NonReturnableQty, out var n) ? n : 0;

            if (returnableQty <= 0 && nonReturnableQty <= 0)
            {
                ErrorMessage = "Укажите количество возвратного и/или невозвратного брака.";
                return false;
            }

            if (returnableQty + nonReturnableQty > item.Quantity)
            {
                ErrorMessage = $"Сумма количеств не может превышать остаток ({item.Quantity} {item.MeasuringUnit}).";
                return false;
            }

            item.IsProcessing = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new DisposalProcessRequest
                {
                    DisposalWarehouseId = _disposalWarehouse.Id,
                    MaterialId = item.MaterialId,
                    ProductId = item.ProductId,
                    ReturnableQuantity = returnableQty,
                    NonReturnableQuantity = nonReturnableQty
                };

                var result = await _apiService.ProcessDisposalAsync(request);

                if (result.IsSuccess)
                {
                    await LoadDataAsync();
                    return true;
                }

                ErrorMessage = result.Message ?? "Не удалось обработать утиль";
                return false;
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

        public async Task<bool> ApproveDisposalRequestAsync(DisposalRequest request)
        {
            try
            {
                var result = await _apiService.ApproveDisposalRequestAsync(request.Id);
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

        public async Task<bool> RejectDisposalRequestAsync(DisposalRequest request)
        {
            try
            {
                var result = await _apiService.RejectDisposalRequestAsync(request.Id);
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _searchText = string.Empty;
        private User? _selectedUser;
        private Product? _selectedProduct;
        private Material? _selectedMaterial;
        private Warehouse? _selectedWarehouse;
        private Dictionary<int, string> _userMap = new();
        private Dictionary<int, string> _productMap = new();
        private Dictionary<int, string> _materialMap = new();
        private Dictionary<int, string> _warehouseMap = new();
        private Dictionary<int, string> _productUnitMap = new();
        private Dictionary<int, string> _materialUnitMap = new();

        public ObservableCollection<HistoryEvent> Events { get; } = new();
        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Material> Materials { get; } = new();
        public ObservableCollection<Warehouse> Warehouses { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set { _selectedUser = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        public Material? SelectedMaterial
        {
            get => _selectedMaterial;
            set { _selectedMaterial = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        public Warehouse? SelectedWarehouse
        {
            get => _selectedWarehouse;
            set { _selectedWarehouse = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        public HistoryViewModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadFiltersDataAsync()
        {
            try
            {
                var usersTask = _apiService.GetAllUsersAsync();
                var productsTask = _apiService.GetAllProductsAsync();
                var materialsTask = _apiService.GetAllMaterialsAsync();
                var warehousesTask = _apiService.GetAllWarehousesAsync();

                await Task.WhenAll(usersTask, productsTask, materialsTask, warehousesTask);

                Users.Clear();
                var users = usersTask.Result ?? new List<User>();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
                _userMap = users.ToDictionary(u => u.Id, u => $"{u.Name} {u.Surname}");

                Products.Clear();
                var products = productsTask.Result ?? new List<Product>();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
                _productMap = products.ToDictionary(p => p.Id, p => p.Name);
                _productUnitMap = products.ToDictionary(p => p.Id, p => p.MeasuringUnit);

                Materials.Clear();
                var materials = materialsTask.Result ?? new List<Material>();
                foreach (var material in materials)
                {
                    Materials.Add(material);
                }
                _materialMap = materials.ToDictionary(m => m.Id, m => m.Name);
                _materialUnitMap = materials.ToDictionary(m => m.Id, m => m.MeasuringUnit);

                Warehouses.Clear();
                var warehouses = warehousesTask.Result ?? new List<Warehouse>();
                foreach (var warehouse in warehouses)
                {
                    Warehouses.Add(warehouse);
                }
                _warehouseMap = warehouses.ToDictionary(w => w.Id, w => w.Name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading filter data: {ex.Message}");
            }
        }

        public async Task LoadHistoryAsync()
        {
            await ApplyFiltersAsync();
        }

        public async Task ApplyFiltersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.GetHistoryAsync(
                    userId: SelectedUser?.Id,
                    warehouseId: SelectedWarehouse?.Id,
                    materialId: SelectedMaterial?.Id,
                    productId: SelectedProduct?.Id
                );

                Events.Clear();

                if (result == null)
                {
                    ErrorMessage = "Не удалось загрузить историю.";
                    return;
                }

                if (_userMap.Count == 0 || _productMap.Count == 0 || _materialMap.Count == 0 || _warehouseMap.Count == 0 ||
                    _productUnitMap.Count == 0 || _materialUnitMap.Count == 0)
                {
                    await LoadFiltersDataAsync();
                }

                var mappedEvents = result.Select(historyEvent =>
                {
                    historyEvent.ActorDisplay = _userMap.TryGetValue(historyEvent.UserId, out var name)
                        ? name
                        : $"ID {historyEvent.UserId}";
                    historyEvent.DisplayDescription = BuildDisplayDescription(historyEvent);
                    return historyEvent;
                });

                // Фильтруем по тексту поиска на клиенте
                var filteredEvents = mappedEvents;
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLowerInvariant();
                    filteredEvents = filteredEvents.Where(e =>
                        (e.DisplayDescription?.ToLowerInvariant().Contains(search) ?? false) ||
                        (e.Action?.ToLowerInvariant().Contains(search) ?? false));
                }

                foreach (var historyEvent in filteredEvents)
                {
                    Events.Add(historyEvent);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ResetFilters()
        {
            _selectedUser = null;
            _selectedProduct = null;
            _selectedMaterial = null;
            _selectedWarehouse = null;
            _searchText = string.Empty;

            OnPropertyChanged(nameof(SelectedUser));
            OnPropertyChanged(nameof(SelectedProduct));
            OnPropertyChanged(nameof(SelectedMaterial));
            OnPropertyChanged(nameof(SelectedWarehouse));
            OnPropertyChanged(nameof(SearchText));

            _ = ApplyFiltersAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string BuildDisplayDescription(HistoryEvent historyEvent)
        {
            var description = historyEvent.Description ?? string.Empty;
            var materialName = ResolveName(_materialMap, historyEvent.MaterialId);
            var productName = ResolveName(_productMap, historyEvent.ProductId);
            var warehouseName = ResolveName(_warehouseMap, historyEvent.WarehouseId);
            var relatedUserName = ResolveName(_userMap, historyEvent.RelatedUserId);
            var actorName = ResolveName(_userMap, historyEvent.UserId);
            var quantity = ExtractQuantity(description);
            var materialUnit = ResolveUnit(_materialUnitMap, historyEvent.MaterialId);
            var productUnit = ResolveUnit(_productUnitMap, historyEvent.ProductId);

            switch (historyEvent.Action)
            {
                case "User.Created":
                    return $"Создан пользователь: {relatedUserName}";
                case "User.Updated":
                    return $"Обновлен пользователь: {relatedUserName}";
                case "User.Deleted":
                    return $"Удален пользователь: {relatedUserName}";
                case "Material.Deleted":
                    return $"Удален материал: {materialName}";
                case "Product.Deleted":
                    return $"Удален продукт: {productName}";
                case "Warehouse.Deleted":
                    return $"Удален склад: {warehouseName}";
                case "PartRequest.Created":
                    return BuildPartRequestDescription("Создан запрос на материал", materialName, quantity, materialUnit);
                case "PartRequest.Approved":
                    return BuildPartRequestDescription("Одобрен запрос на материал", materialName, null, materialUnit);
                case "PartRequest.Rejected":
                    return BuildPartRequestDescription("Отклонен запрос на материал", materialName, null, materialUnit);
                case "PartRequest.Cancelled":
                    return BuildPartRequestDescription("Отменен запрос на материал", materialName, null, materialUnit);
                case "PartRequest.Deleted":
                    return BuildPartRequestDescription("Удален запрос на материал", materialName, null, materialUnit);
                case "Stock.Created":
                    return BuildStockDescription("Добавлен остаток", materialName, productName, warehouseName, quantity, materialUnit, productUnit);
                case "Stock.Updated":
                    return BuildStockDescription("Обновлен остаток", materialName, productName, warehouseName, quantity, materialUnit, productUnit);
                case "Stock.Deleted":
                    return BuildStockDescription("Удален остаток", materialName, productName, warehouseName, null, materialUnit, productUnit);
                case "Stock.QuantityUpdated":
                    return BuildStockDescription("Изменено количество", materialName, productName, warehouseName, quantity, materialUnit, productUnit);
                case "ShiftTransfer.Created":
                    return BuildShiftTransferDescription("Создана передача смены", actorName, relatedUserName);
                case "ShiftTransfer.Confirmed":
                    return BuildShiftTransferDescription("Подтверждена передача смены", relatedUserName, actorName);
                case "ShiftTransfer.Cancelled":
                    return BuildShiftTransferDescription("Отменена передача смены", actorName, relatedUserName);
                case "ShiftTransfer.Deleted":
                    return "Удалена передача смены";
                case "Shift.Started":
                    return "Начало смены";
                case "Shift.Finished":
                    return "Завершение смены";
                case "Reprocessing.Created":
                    return BuildReprocessingDescription(materialName, warehouseName, quantity, materialUnit);
            }

            if (historyEvent.Action is "Material.Created" or "Material.Updated" ||
                historyEvent.Action is "Product.Created" or "Product.Updated" ||
                historyEvent.Action is "Warehouse.Created" or "Warehouse.Updated" ||
                historyEvent.Action is "Warehouse.Stopped" or "Warehouse.Started" ||
                historyEvent.Action is "AlarmEvent.Created")
            {
                return description;
            }

            return description;
        }

        private static string ResolveName(Dictionary<int, string> map, int? id)
        {
            if (id.HasValue && map.TryGetValue(id.Value, out var name))
            {
                return name;
            }

            return id.HasValue ? $"ID {id.Value}" : "неизвестно";
        }

        private static string? ExtractQuantity(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            var match = Regex.Match(description, "кол-во\\s+([0-9]+([.,][0-9]+)?)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string BuildPartRequestDescription(string prefix, string materialName, string? quantity, string? unit)
        {
            return quantity == null
                ? $"{prefix} {materialName}"
                : $"{prefix} {materialName} (кол-во {quantity}{FormatUnit(unit)})";
        }

        private static string BuildStockDescription(
            string prefix,
            string materialName,
            string productName,
            string warehouseName,
            string? quantity,
            string? materialUnit,
            string? productUnit)
        {
            var itemName = materialName != "неизвестно" ? materialName : productName;
            var unit = materialName != "неизвестно" ? materialUnit : productUnit;
            var baseText = $"{prefix} {itemName} на складе {warehouseName}";
            return quantity == null ? baseText : $"{baseText} (кол-во {quantity}{FormatUnit(unit)})";
        }

        private static string BuildShiftTransferDescription(string prefix, string fromUser, string toUser)
        {
            return $"{prefix} (от {fromUser} к {toUser})";
        }

        private static string BuildReprocessingDescription(string materialName, string warehouseName, string? quantity, string? unit)
        {
            var baseText = $"Производство материала {materialName}";
            if (warehouseName != "неизвестно")
            {
                baseText += $" на складе {warehouseName}";
            }
            return quantity == null ? baseText : $"{baseText} (кол-во {quantity}{FormatUnit(unit)})";
        }

        private static string? ResolveUnit(Dictionary<int, string> map, int? id)
        {
            if (id.HasValue && map.TryGetValue(id.Value, out var unit) && !string.IsNullOrWhiteSpace(unit))
            {
                return unit;
            }

            return id.HasValue ? "шт" : null;
        }

        private static string FormatUnit(string? unit)
        {
            return string.IsNullOrWhiteSpace(unit) ? string.Empty : $" {unit}";
        }
    }
}

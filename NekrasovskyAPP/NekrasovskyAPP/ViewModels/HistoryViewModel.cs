using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
                foreach (var product in productsTask.Result ?? new List<Product>())
                {
                    Products.Add(product);
                }

                Materials.Clear();
                foreach (var material in materialsTask.Result ?? new List<Material>())
                {
                    Materials.Add(material);
                }

                Warehouses.Clear();
                foreach (var warehouse in warehousesTask.Result ?? new List<Warehouse>())
                {
                    Warehouses.Add(warehouse);
                }
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

                // Если карта пользователей пуста, загрузим её
                if (_userMap.Count == 0)
                {
                    var users = await _apiService.GetAllUsersAsync();
                    _userMap = users?.ToDictionary(u => u.Id, u => $"{u.Name} {u.Surname}") ?? new Dictionary<int, string>();
                }

                // Фильтруем по тексту поиска на клиенте
                var filteredEvents = result.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLowerInvariant();
                    filteredEvents = filteredEvents.Where(e =>
                        (e.Description?.ToLowerInvariant().Contains(search) ?? false) ||
                        (e.Action?.ToLowerInvariant().Contains(search) ?? false));
                }

                foreach (var historyEvent in filteredEvents)
                {
                    if (_userMap.TryGetValue(historyEvent.UserId, out var name))
                    {
                        historyEvent.ActorDisplay = $"{name}";
                    }
                    else
                    {
                        historyEvent.ActorDisplay = $"ID {historyEvent.UserId}";
                    }
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
    }
}

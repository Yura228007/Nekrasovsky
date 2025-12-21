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
        }

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Product> Products { get; }
        public ObservableCollection<Material> Materials { get; }
        public ObservableCollection<Warehouse> Warehouses { get; }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


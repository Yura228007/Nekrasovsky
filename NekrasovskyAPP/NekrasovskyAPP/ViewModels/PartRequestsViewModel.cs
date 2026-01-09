using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class PartRequestsViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public PartRequestsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            PartRequests = new ObservableCollection<PartRequest>();
            Users = new ObservableCollection<User>();
            Warehouses = new ObservableCollection<Warehouse>();
            Materials = new ObservableCollection<Material>();
        }

        public ObservableCollection<PartRequest> PartRequests { get; }
        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Warehouse> Warehouses { get; }
        public ObservableCollection<Material> Materials { get; }

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

        public async Task LoadPartRequestsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var requests = await _apiService.GetAllPartRequestsAsync();
                PartRequests.Clear();
                foreach (var request in requests)
                {
                    PartRequests.Add(request);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки запросов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ApprovePartRequestAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.ApprovePartRequestAsync(id);
                if (response.GetData() != null)
                {
                    await LoadPartRequestsAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка одобрения запроса";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка одобрения запроса: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RejectPartRequestAsync(int id, string? reason = null)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.RejectPartRequestAsync(id, reason);
                if (response.GetData() != null)
                {
                    await LoadPartRequestsAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка отклонения запроса";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка отклонения запроса: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadDependenciesAsync()
        {
            try
            {
                var users = await _apiService.GetAllUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                var warehouses = await _apiService.GetAllWarehousesAsync();
                Warehouses.Clear();
                foreach (var warehouse in warehouses)
                {
                    Warehouses.Add(warehouse);
                }

                var materials = await _apiService.GetAllMaterialsAsync();
                Materials.Clear();
                foreach (var material in materials)
                {
                    Materials.Add(material);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки данных: {ex.Message}";
            }
        }

        public async Task<bool> CreatePartRequestAsync(PartRequest request)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.AddPartRequestAsync(request);
                if (response.GetData() != null)
                {
                    await LoadPartRequestsAsync();
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка создания запроса";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания запроса: {ex.Message}";
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




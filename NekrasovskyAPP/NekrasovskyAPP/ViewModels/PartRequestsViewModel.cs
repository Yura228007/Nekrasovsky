using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class PartRequestsViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public PartRequestsViewModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
            IncomingRequests = new ObservableCollection<PartRequest>();
            SentRequests = new ObservableCollection<PartRequest>();
            Users = new ObservableCollection<User>();
            Warehouses = new ObservableCollection<Warehouse>();
            Materials = new ObservableCollection<Material>();
        }

        public ObservableCollection<PartRequest> IncomingRequests { get; }
        public ObservableCollection<PartRequest> SentRequests { get; }
        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Warehouse> Warehouses { get; }
        public ObservableCollection<Material> Materials { get; }
        public User? CurrentUser => _authService.CurrentUser;

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
                var currentUser = _authService.CurrentUser;
                if (currentUser == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    return;
                }

                var incoming = await _apiService.GetPartRequestsByUserAsync(currentUser.Id, sent: false);
                IncomingRequests.Clear();
                foreach (var request in incoming)
                {
                    IncomingRequests.Add(request);
                }

                var sent = await _apiService.GetPartRequestsByUserAsync(currentUser.Id, sent: true);
                SentRequests.Clear();
                foreach (var request in sent)
                {
                    SentRequests.Add(request);
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

        public async Task CancelPartRequestAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.CancelPartRequestAsync(id);
                if (!string.IsNullOrEmpty(response.Message) &&
                    response.Message.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = response.Message;
                }
                else
                {
                    await LoadPartRequestsAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка отмены запроса: {ex.Message}";
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

                var warehouses = (await _apiService.GetAllWarehousesAsync())
                    .Where(w => w.IsActive)
                    .ToList();
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




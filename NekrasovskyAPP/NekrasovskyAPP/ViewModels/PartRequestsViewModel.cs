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
        }

        public ObservableCollection<PartRequest> PartRequests { get; }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}




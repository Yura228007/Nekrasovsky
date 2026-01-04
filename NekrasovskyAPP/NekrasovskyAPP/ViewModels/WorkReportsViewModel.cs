using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class WorkReportsViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public WorkReportsViewModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
            WorkReports = new ObservableCollection<WorkReport>();
        }

        public ObservableCollection<WorkReport> WorkReports { get; }

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

        public async Task LoadWorkReportsAsync()
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

                var reports = await _apiService.GetWorkReportsByUserAsync(currentUser.Id);
                WorkReports.Clear();
                foreach (var report in reports)
                {
                    WorkReports.Add(report);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки отчетов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task StartWorkAsync()
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

                var request = new Services.StartWorkRequest
                {
                    UserId = currentUser.Id,
                    StartTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                var response = await _apiService.StartWorkAsync(request);
                if (response.GetData() != null)
                {
                    await LoadWorkReportsAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка начала работы";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка начала работы: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task FinishWorkAsync(int reportId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new Services.FinishWorkRequest
                {
                    FinishTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                var response = await _apiService.FinishWorkAsync(reportId, request);
                if (response.GetData() != null)
                {
                    await LoadWorkReportsAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка завершения работы";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка завершения работы: {ex.Message}";
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


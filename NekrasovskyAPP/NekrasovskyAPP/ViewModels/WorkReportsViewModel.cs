using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System.Threading;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
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
        private bool _canManageShiftManually = true;

        public WorkReportsViewModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
            WorkReports = new ObservableCollection<WorkReport>();
        }

        public ObservableCollection<WorkReport> WorkReports { get; }

        public bool CanManageShiftManually
        {
            get => _canManageShiftManually;
            private set
            {
                _canManageShiftManually = value;
                OnPropertyChanged();
            }
        }

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

                await RefreshShiftPermissionsAsync(currentUser);

                List<WorkReport> reports;
                if (IsPrivilegedUser(currentUser))
                {
                    reports = await _apiService.GetAllWorkReportsAsync();
                }
                else
                {
                    reports = await _apiService.GetWorkReportsByUserAsync(currentUser.Id);
                }

                WorkReports.Clear();
                foreach (var report in reports.OrderByDescending(r => r.Date).ThenByDescending(r => r.StartWork))
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

                if (!CanManageShiftManually)
                {
                    ErrorMessage = "Ручной старт смены недоступен для вашей роли";
                    return;
                }

                var request = new Services.StartWorkRequest
                {
                    UserId = currentUser.Id,
                    StartTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
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
                ErrorMessage = string.Empty;

                if (!CanManageShiftManually)
                {
                    ErrorMessage = "Ручное завершение смены недоступно для вашей роли";
                    return;
                }

                // Запрашиваем примечание у пользователя
                string? note = null;
                if (Application.Current?.MainPage != null)
                {
                    note = await Application.Current.MainPage.DisplayPromptAsync(
                        "Завершение смены",
                        "Введите примечание (необязательно):",
                        accept: "Завершить",
                        cancel: "Отмена",
                        placeholder: "Примечание...",
                        maxLength: 500);

                    // Если пользователь нажал "Отмена", выходим
                    if (note == null)
                    {
                        return;
                    }
                }

                IsLoading = true;

                var request = new Services.FinishWorkRequest
                {
                    FinishTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
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

        public async Task DownloadReportAsync(int workReportId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Get shift report by work report ID
                var shiftReport = await _apiService.GetShiftReportByWorkReportIdAsync(workReportId);
                if (shiftReport == null)
                {
                    ErrorMessage = "Отчет смены не найден";
                    return;
                }

                // Download the report file
                var fileBytes = await _apiService.DownloadShiftReportAsync(shiftReport.Id);
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    ErrorMessage = "Не удалось скачать файл отчета";
                    return;
                }

                var fileName = shiftReport.FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"ShiftReport_{workReportId}.xlsx";
                }

                // Диалог «Сохранить как» — пользователь выбирает папку и имя файла (ПК и телефон)
                using var stream = new MemoryStream(fileBytes);
                var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

                if (result.IsSuccessful)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Успешно",
                            $"Отчет сохранен: {result.FilePath ?? fileName}",
                            "OK");
                    }
                }
                else if (result.Exception != null)
                {
                    ErrorMessage = $"Ошибка сохранения: {result.Exception.Message}";
                }
                // Если пользователь отменил диалог — ничего не показываем
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка скачивания отчета: {ex.Message}";
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

        private async Task RefreshShiftPermissionsAsync(User user)
        {
            if (IsPrivilegedUser(user))
            {
                CanManageShiftManually = true;
                return;
            }

            var hasShiftTransferPermission = false;
            if (user.RoleId.HasValue)
            {
                var rolePermissions = await _apiService.GetRolePermissionsAsync(user.RoleId.Value);
                hasShiftTransferPermission = rolePermissions.Any(p => p.Code == "ShiftTransfer");
            }

            if (!hasShiftTransferPermission)
            {
                var userPermissions = await _apiService.GetUserPermissionsAsync(user.Id);
                hasShiftTransferPermission = userPermissions.Any(p => p.Code == "ShiftTransfer");
            }

            CanManageShiftManually = !hasShiftTransferPermission;
        }

        private static bool IsPrivilegedUser(User user)
        {
            if (user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var roleCode = user.Role?.Code ?? string.Empty;
            var roleName = user.Role?.Name ?? string.Empty;
            return roleCode.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                   roleCode.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   roleCode.Equals("SeniorExtruder", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Equals("Владелец", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Equals("Администратор", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Equals("Старший экструзионщик", StringComparison.OrdinalIgnoreCase);
        }
    }
}




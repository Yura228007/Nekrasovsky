using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class ShiftTransfersViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public ShiftTransfersViewModel(IApiService apiService, IAuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
            SentTransfers = new ObservableCollection<ShiftTransfer>();
            PendingTransfers = new ObservableCollection<ShiftTransfer>();
            Users = new ObservableCollection<User>();
        }

        public ObservableCollection<ShiftTransfer> SentTransfers { get; }
        public ObservableCollection<ShiftTransfer> PendingTransfers { get; }
        public ObservableCollection<User> Users { get; }

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

        public async Task LoadTransfersAsync()
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

                var sent = await _apiService.GetShiftTransfersByUserAsync(currentUser.Id, sent: true);
                SentTransfers.Clear();
                foreach (var transfer in sent)
                {
                    SentTransfers.Add(transfer);
                }

                var pending = await _apiService.GetPendingShiftTransfersAsync(currentUser.Id);
                PendingTransfers.Clear();
                foreach (var transfer in pending)
                {
                    PendingTransfers.Add(transfer);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки передач смены: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> CreateShiftTransferAsync(int toUserId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var currentUser = _authService.CurrentUser;
                if (currentUser == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    return false;
                }

                var transfer = new ShiftTransfer
                {
                    FromUserId = currentUser.Id,
                    ToUserId = toUserId,
                    TransferDate = DateTime.UtcNow,
                    IsConfirmed = false
                };

                var response = await _apiService.CreateShiftTransferAsync(transfer);
                if (response.GetData() != null)
                {
                    await LoadTransfersAsync();
                    return true;
                }

                ErrorMessage = response.Message ?? "Ошибка передачи смены";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка передачи смены: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> ConfirmShiftTransferAsync(int transferId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.ConfirmShiftTransferAsync(transferId);
                if (response.GetData() != null)
                {
                    await LoadTransfersAsync();
                    return true;
                }

                ErrorMessage = response.Message ?? "Ошибка подтверждения передачи смены";
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка подтверждения передачи смены: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> CancelShiftTransferAsync(int transferId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.CancelShiftTransferAsync(transferId);
                if (!string.IsNullOrEmpty(response.Message) &&
                    response.Message.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = response.Message;
                    return false;
                }

                await LoadTransfersAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка отмены передачи смены: {ex.Message}";
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

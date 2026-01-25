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

        public ObservableCollection<HistoryEvent> Events { get; } = new();

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

        public HistoryViewModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadHistoryAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.GetHistoryAsync();
                Events.Clear();

                if (result == null)
                {
                    ErrorMessage = "Не удалось загрузить историю. Проверьте соединение с сервером.";
                    return;
                }

                var users = await _apiService.GetAllUsersAsync();
                var userMap = users?.ToDictionary(u => u.Id, u => $"{u.Name} {u.Surname}") ?? new Dictionary<int, string>();

                foreach (var historyEvent in result)
                {
                    if (userMap.TryGetValue(historyEvent.UserId, out var name))
                    {
                        historyEvent.ActorDisplay = $"{name} (ID {historyEvent.UserId})";
                    }
                    else
                    {
                        historyEvent.ActorDisplay = $"Пользователь ID {historyEvent.UserId}";
                    }
                    Events.Add(historyEvent);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки истории: {ex.Message}";
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

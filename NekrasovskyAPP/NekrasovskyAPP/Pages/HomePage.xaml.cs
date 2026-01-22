using NekrasovskyAPP.Services;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class HomePage : ContentPage
    {
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private readonly ISignalRService _signalRService;
        private readonly IAlarmSoundService _alarmSoundService;

        public HomePage(IAuthService authService, IApiService apiService, ISignalRService signalRService, IAlarmSoundService alarmSoundService)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
            _signalRService = signalRService;
            _alarmSoundService = alarmSoundService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Подключаемся к SignalR при открытии страницы
            if (!_signalRService.IsConnected)
            {
                try
                {
                    await _signalRService.ConnectAsync();
                    SetupSignalRHandlers();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка подключения к SignalR: {ex.Message}");
                }
            }
            else
            {
                SetupSignalRHandlers();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Не отключаемся от SignalR, чтобы получать уведомления даже когда страница не активна
        }

        private void SetupSignalRHandlers()
        {
            // Настраиваем обработчик уведомлений через SignalRService
            _signalRService.SetOnAlarmNotification((message, location, user) =>
            {
                // Воспроизводим звук на главном потоке
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _alarmSoundService.PlayAlarmSound();

                    // Показываем уведомление
                    DisplayAlert(
                        "🚨 ТРЕВОГА!",
                        $"{message}\n\nМесто: {location}\nПользователь: {user}",
                        "OK");
                });
            });
        }

        private async void OnUsersClicked(object sender, EventArgs e)
        {
            // Относительная навигация к глобальному маршруту (без слешей)
            await Shell.Current.GoToAsync("UsersPage");
        }

        private async void OnProductsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductsPage");
        }

        private async void OnMaterialsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("MaterialsPage");
        }

        private async void OnWarehousesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("WarehousesPage");
        }

        private async void OnWorkReportsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("WorkReportsPage");
        }

        private async void OnPartRequestsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("PartRequestsPage");
        }

        private async void OnShiftTransfersClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ShiftTransfersPage");
        }

        private async void OnAlarmClicked(object sender, EventArgs e)
        {
            try
            {
                if (_authService.CurrentUser == null)
                {
                    await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                    return;
                }

                // Запрашиваем местоположение и сообщение
                var location = await DisplayPromptAsync(
                    "🚨 ТРЕВОГА",
                    "Укажите место происшествия:",
                    "Отправить",
                    "Отмена",
                    "Место",
                    -1,
                    Keyboard.Default);

                if (string.IsNullOrWhiteSpace(location))
                {
                    return; // Пользователь отменил
                }

                var message = await DisplayPromptAsync(
                    "🚨 ТРЕВОГА",
                    "Опишите ситуацию (необязательно):",
                    "Отправить",
                    "Пропустить",
                    "Сообщение",
                    -1,
                    Keyboard.Default);

                // Создаем событие тревоги
                var alarmEvent = new AlarmEvent
                {
                    UserId = _authService.CurrentUser.Id,
                    Location = location,
                    Message = message ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                // Отправляем на сервер
                var response = await _apiService.AddAlarmEventAsync(alarmEvent);
                
                if (response.AlarmEvent != null)
                {
                    await DisplayAlert("Успех", "Тревога отправлена! Все пользователи получат уведомление.", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", response.Message ?? "Не удалось отправить тревогу", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Отключаемся от SignalR при выходе
            try
            {
                await _signalRService.DisconnectAsync();
            }
            catch { }

            // Выполняем выход
            _authService.Logout();
            
            // Навигация к LoginPage и сброс стека навигации
            await Shell.Current.GoToAsync("///LoginPage");
        }
    }
}


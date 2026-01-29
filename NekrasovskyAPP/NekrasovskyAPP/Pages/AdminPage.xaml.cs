using System;
using Microsoft.Maui.ApplicationModel;
using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Services;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class AdminPage : ContentPage
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private readonly ISignalRService _signalRService;
        private readonly IAlarmSoundService _alarmSoundService;

        public string CurrentUserName => _authService.CurrentUser != null 
            ? $"Добро пожаловать, {_authService.CurrentUser.Name} {_authService.CurrentUser.Surname}!"
            : "";

        public AdminPage(MainViewModel mainViewModel, IAuthService authService, IApiService apiService, ISignalRService signalRService, IAlarmSoundService alarmSoundService)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            _authService = authService;
            _apiService = apiService;
            _signalRService = signalRService;
            _alarmSoundService = alarmSoundService;

            BindingContext = this;
        }
        

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SubscribeToAuthServiceEvents();

            // Показать карточку управления ролями только для Owner
            UpdateRolePermissionsCardVisibility();

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

        private void UpdateRolePermissionsCardVisibility()
        {
            var user = _authService.CurrentUser;
            var isOwner = user?.Role?.Code?.Equals("Owner", StringComparison.OrdinalIgnoreCase) == true;
            RolePermissionsCard.IsVisible = isOwner;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _authService.CurrentUserChanged -= OnCurrentUserChanged;
        }

        private void SubscribeToAuthServiceEvents()
        {
            _authService.CurrentUserChanged -= OnCurrentUserChanged;
            _authService.CurrentUserChanged += OnCurrentUserChanged;
            RefreshCurrentUserName();
        }

        private void RefreshCurrentUserName()
        {
            OnPropertyChanged(nameof(CurrentUserName));
        }

        private void OnCurrentUserChanged(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(RefreshCurrentUserName);
        }

        private void SetupSignalRHandlers()
        {
            _signalRService.SetOnAlarmNotification((message, location, user) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    _alarmSoundService.PlayAlarmSound();
                    await DisplayAlert(
                        "🚨 ТРЕВОГА!",
                        $"{message}\n\nМесто: {location}\nПользователь: {user}",
                        "OK");
                    _alarmSoundService.StopAlarmSound();
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

        private async void OnReportsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("WorkReportsPage");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Относительная навигация к глобальному маршруту (без слешей)
            await Shell.Current.GoToAsync("SettingsPage");
        }

        private async void OnHistoryClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("HistoryPage");
        }

        private async void OnQrCodesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("QrCodesPage");
        }

        private async void OnRolePermissionsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("RolePermissionsPage");
        }

        private async void OnMachinesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("MachinesPage");
        }

        private async void OnProductOutputClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductOutputPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Выполняем выход
            _authService.Logout();
            
            // Навигация к LoginPage и сброс стека навигации
            await Shell.Current.GoToAsync("///LoginPage");
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
                    return;
                }

                var message = await DisplayPromptAsync(
                    "🚨 ТРЕВОГА",
                    "Опишите ситуацию (необязательно):",
                    "Отправить",
                    "Пропустить",
                    "Сообщение",
                    -1,
                    Keyboard.Default);

                var alarmEvent = new AlarmEvent
                {
                    UserId = _authService.CurrentUser.Id,
                    Location = location,
                    Message = message ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

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
    }
}


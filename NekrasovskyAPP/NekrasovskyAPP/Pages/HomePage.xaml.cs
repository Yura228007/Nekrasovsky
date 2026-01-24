using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
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

            public string CurrentUserName => _authService.CurrentUser != null 
        ? $"Добро пожаловать, {_authService.CurrentUser.Name} {_authService.CurrentUser.Surname}!"
        : "";

        public HomePage(IAuthService authService, IApiService apiService, ISignalRService signalRService, IAlarmSoundService alarmSoundService)
        {
            InitializeComponent();
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
            _authService.CurrentUserChanged -= OnCurrentUserChanged;
            // Не отключаемся от SignalR, чтобы получать уведомления даже когда страница не активна
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
            // Настраиваем обработчик уведомлений через SignalRService
            _signalRService.SetOnAlarmNotification((message, location, user) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Проверяем, не от текущего ли пользователя тревога (чтобы не дублировать)
                    var currentUser = _authService.CurrentUser;
                    if (currentUser != null)
                    {
                        var currentUserFullName = $"{currentUser.Name} {currentUser.Surname}";
                        if (user == currentUserFullName)
                        {
                            // Это наша собственная тревога - уже показали уведомление
                            return;
                        }
                    }

                    _alarmSoundService.PlayAlarmSound();
                    await DisplayAlert(
                        "🚨 ТРЕВОГА!",
                        $"{message}\n\nМесто: {location}\nПользователь: {user}",
                        "OK");
                    _alarmSoundService.StopAlarmSound();
                });
            });
        }

        private async void OnScannerClicked(object sender, EventArgs e)
        {
#if ANDROID || IOS
            try
            {
                var scannerPage = new BarcodeScannerPage();
                scannerPage.BarcodeScanned += OnBarcodeScanned;
                await Navigation.PushModalAsync(scannerPage);
            }
            catch
            {
                await DisplayAlert("Ошибка", "Не удалось открыть сканер. Убедитесь, что приложение имеет разрешение на использование камеры.", "OK");
            }
#else
            await DisplayAlert("Недоступно", "Сканирование штрих-кодов доступно только на Android и iOS устройствах.", "OK");
#endif
        }

        private async void OnBarcodeScanned(object? sender, string barcodeValue)
        {
#if ANDROID || IOS
            if (sender is BarcodeScannerPage scannerPage)
            {
                scannerPage.BarcodeScanned -= OnBarcodeScanned;
            }
#endif

            if (string.IsNullOrWhiteSpace(barcodeValue))
                return;

            try
            {
                // Ищем по артикулу в продуктах
                var products = await _apiService.SearchProductsAsync(null, barcodeValue);
                var product = products.FirstOrDefault(p => p.Code == barcodeValue);

                if (product != null)
                {
                    await DisplayAlert(
                        "Продукт найден",
                        $"Название: {product.Name}\n" +
                        $"Артикул: {product.Code}\n" +
                        $"Описание: {product.Description ?? "Не указано"}\n" +
                        $"Единица измерения: {product.MeasuringUnit}\n" +
                        $"Статус: {(product.IsActive ? "Активен" : "Неактивен")}",
                        "OK");
                    return;
                }

                // Ищем по артикулу в материалах
                var materials = await _apiService.SearchMaterialsAsync(null, barcodeValue);
                var material = materials.FirstOrDefault(m => m.Code == barcodeValue);

                if (material != null)
                {
                    await DisplayAlert(
                        "Материал найден",
                        $"Название: {material.Name}\n" +
                        $"Артикул: {material.Code}\n" +
                        $"Описание: {material.Description ?? "Не указано"}\n" +
                        $"Единица измерения: {material.MeasuringUnit}\n" +
                        $"Статус: {(material.IsActive ? "Активен" : "Неактивен")}",
                        "OK");
                    return;
                }

                // Ничего не найдено
                await DisplayAlert(
                    "Не найдено",
                    $"Артикул '{barcodeValue}' не найден среди продуктов и материалов.",
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при поиске: {ex.Message}", "OK");
            }
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
                    // Воспроизводим звук тревоги для отправителя
                    _alarmSoundService.PlayAlarmSound();

                    await DisplayAlert(
                        "🚨 ТРЕВОГА ОТПРАВЛЕНА!",
                        $"Место: {location}\n" +
                        $"Сообщение: {message ?? "Не указано"}\n\n" +
                        "Все пользователи получат уведомление.",
                        "OK");

                    // Останавливаем звук после закрытия диалога
                    _alarmSoundService.StopAlarmSound();
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


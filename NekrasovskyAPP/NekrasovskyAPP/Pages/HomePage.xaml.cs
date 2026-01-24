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
        private readonly IAlarmNotificationService _alarmNotificationService;

        public string CurrentUserName => _authService.CurrentUser != null
            ? $"Добро пожаловать, {_authService.CurrentUser.Name} {_authService.CurrentUser.Surname}!"
            : "";

        public HomePage(
            IAuthService authService,
            IApiService apiService,
            IAlarmNotificationService alarmNotificationService)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
            _alarmNotificationService = alarmNotificationService;

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SubscribeToAuthServiceEvents();

            // Инициализируем сервис уведомлений
            await _alarmNotificationService.InitializeAsync();
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
            if (_authService.CurrentUser == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            // Запрашиваем местоположение
            var location = await DisplayPromptAsync(
                "🚨 ТРЕВОГА",
                "Укажите место происшествия:",
                "Отправить",
                "Отмена",
                "Место",
                -1,
                Keyboard.Default);

            if (string.IsNullOrWhiteSpace(location))
                return;

            // Запрашиваем сообщение
            var message = await DisplayPromptAsync(
                "🚨 ТРЕВОГА",
                "Опишите ситуацию (необязательно):",
                "Отправить",
                "Пропустить",
                "Сообщение",
                -1,
                Keyboard.Default);

            // Отправляем тревогу через сервис
            var success = await _alarmNotificationService.SendAlarmAsync(location, message);

            if (!success)
            {
                await DisplayAlert("Ошибка", "Не удалось отправить тревогу", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _authService.Logout();
            await Shell.Current.GoToAsync("///LoginPage");
        }
    }
}

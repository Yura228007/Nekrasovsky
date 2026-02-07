using System;
using System.Linq;
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
        private bool _isPrivilegedUser;
        private bool _hasSendToSalePermission;
        private bool _hasAssignBarcodePermission;

        public string CurrentUserName => _authService.CurrentUser != null 
            ? $"{_authService.CurrentUser.Surname} {_authService.CurrentUser.Name}"
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
            
            // Обновить видимость карточек в зависимости от прав
            await RefreshPermissionsAsync();

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

        private async Task RefreshPermissionsAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                _isPrivilegedUser = false;
                _hasSendToSalePermission = false;
                _hasAssignBarcodePermission = false;
                UpdateCardVisibility();
                return;
            }

            try
            {
                _isPrivilegedUser = IsPrivilegedUser(currentUser);
                _hasSendToSalePermission = await CheckPermissionAsync(currentUser, "SendToSale");
                _hasAssignBarcodePermission = await CheckPermissionAsync(currentUser, "AssignBarcode");
                UpdateCardVisibility();
            }
            catch
            {
                _isPrivilegedUser = false;
                _hasSendToSalePermission = false;
                _hasAssignBarcodePermission = false;
                UpdateCardVisibility();
            }
        }

        private async Task<bool> CheckPermissionAsync(User user, string permissionCode)
        {
            if (_isPrivilegedUser) return true;

            if (user.RoleId.HasValue)
            {
                var rolePermissions = await _apiService.GetRolePermissionsAsync(user.RoleId.Value);
                if (rolePermissions.Any(p => p.Code == permissionCode))
                {
                    return true;
                }
            }

            var userPermissions = await _apiService.GetUserPermissionsAsync(user.Id);
            return userPermissions.Any(p => p.Code == permissionCode);
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

        private void UpdateCardVisibility()
        {
            // Сканер - только с правом AssignBarcode
            ScannerCard.IsVisible = _hasAssignBarcodePermission || _isPrivilegedUser;
            
            // Упаковка - только с правом SendToSale
            ProductOutputCard.IsVisible = _hasSendToSalePermission || _isPrivilegedUser;
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
            MainThread.BeginInvokeOnMainThread(async () => await RefreshPermissionsAsync());
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

        private async void OnProductBatchesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductBatchesPage");
        }

        private async void OnProductCatalogClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductCatalogPage");
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

        private async void OnPartRequestsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("PartRequestsPage");
        }

        private async void OnShiftTransfersClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ShiftTransfersPage");
        }

        private async void OnReprocessingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ReprocessingPage");
        }

        private async void OnDisposalClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("DisposalPage");
        }

        private async void OnFinishedGoodsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("FinishedGoodsPage");
        }

        private async void OnSDHClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("SDHPage");
        }

        private async void OnMixingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("MixingPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _authService.Logout();
            HomePage.ResetLockDialogShown();
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


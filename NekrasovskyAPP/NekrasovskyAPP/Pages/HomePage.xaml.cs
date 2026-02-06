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
        private readonly IDeviceLockService _deviceLockService;
        private readonly ISignalRService _signalRService;
        private bool _hasActiveShift;
        private bool _hasShiftTransferPermission;
        private bool _isPrivilegedUser;
        private bool _hasAssignBarcodePermission;
        private bool _hasManageRecipesPermission;
        private bool _hasDisposalPermission;
        private bool _hasWriteOffPermission;
        private bool _hasSendToSalePermission;
        private bool _hasManageFinishedGoodsPermission;
        private static bool _lockDialogShownThisSession;
        private static bool _unlockDeviceHandlerSet;

        /// <summary>Сбросить флаг показа диалога блокировки (вызывать при выходе, чтобы при следующем входе диалог снова показался).</summary>
        public static void ResetLockDialogShown()
        {
            _lockDialogShownThisSession = false;
        }

        public string CurrentUserName => _authService.CurrentUser != null
            ? $"{_authService.CurrentUser.Surname} {_authService.CurrentUser.Name}"
            : "";

        public HomePage(
            IAuthService authService,
            IApiService apiService,
            IAlarmNotificationService alarmNotificationService,
            IDeviceLockService deviceLockService,
            ISignalRService signalRService)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
            _alarmNotificationService = alarmNotificationService;
            _deviceLockService = deviceLockService;
            _signalRService = signalRService;

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SubscribeToAuthServiceEvents();

            // Инициализируем сервис уведомлений
            await _alarmNotificationService.InitializeAsync();
            await RefreshShiftStateAsync();

            // Регистрация в SignalR для команды разблокировки с админ-панели и диалог блокировки устройства (только Android, не админ)
            await SetupDeviceLockAndSignalRAsync();
        }

        private async Task SetupDeviceLockAndSignalRAsync()
        {
            var user = _authService.CurrentUser;
            if (user == null) return;

            // Подключаем SignalR и регистрируем userId, чтобы админ мог отправить команду "Закрыть"
            if (!_signalRService.IsConnected)
            {
                try
                {
                    await _signalRService.ConnectAsync();
                    await _signalRService.SubscribeToAlarmNotificationsAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR connect error: {ex.Message}");
                }
            }

            await _signalRService.RegisterUserIdAsync(user.Id);

            if (!_unlockDeviceHandlerSet)
            {
                _unlockDeviceHandlerSet = true;
                _signalRService.SetOnUnlockDevice(() =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _deviceLockService.UnlockAndCloseAsync();
                    });
                });
            }

            // Диалог "Заблокировать устройство?" — только на Android, только для не-админа, один раз за сессию
            if (_lockDialogShownThisSession) return;
#if ANDROID
            if (!_deviceLockService.IsLockSupported) return;
            var isAdmin = await _authService.IsAdminAsync(user.Id);
            if (isAdmin) return;

            _lockDialogShownThisSession = true;
            var block = await DisplayAlert(
                "Заблокировать устройство?",
                "При подтверждении выход из приложения будет заблокирован. Разблокировка возможна только с панели администратора (кнопка «Закрыть» у пользователя).",
                "Заблокировать",
                "Нет");

            if (block)
            {
                await _deviceLockService.LockAsync();
            }
#endif
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
            MainThread.BeginInvokeOnMainThread(async () => await RefreshShiftStateAsync());
        }

        private async void OnScannerClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("Scanner"))
            {
                return;
            }
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
            if (!await EnsureShiftAccessAsync("ProductsPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("ProductsPage");
        }

        private async void OnProductBatchesClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("ProductBatchesPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("ProductBatchesPage");
        }

        private async void OnMaterialsClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("MaterialsPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("MaterialsPage");
        }

        private async void OnWarehousesClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("WarehousesPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("WarehousesPage");
        }

        private async void OnWorkReportsClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("WorkReportsPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("WorkReportsPage");
        }

        private async void OnPartRequestsClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("PartRequestsPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("PartRequestsPage");
        }

        private async void OnShiftTransfersClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ShiftTransfersPage");
        }

        private async void OnReprocessingClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("ReprocessingPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("ReprocessingPage");
        }

        private async void OnMixingClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("MixingPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("MixingPage");
        }

        private async void OnDisposalClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("DisposalPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("DisposalPage");
        }

        private async void OnProductOutputClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductOutputPage");
        }

        private async void OnFinishedGoodsClicked(object sender, EventArgs e)
        {
            if (!await EnsureShiftAccessAsync("FinishedGoodsPage"))
            {
                return;
            }
            await Shell.Current.GoToAsync("FinishedGoodsPage");
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
            ResetLockDialogShown();
            await Shell.Current.GoToAsync("///LoginPage");
        }

        private async Task RefreshShiftStateAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                _hasActiveShift = false;
                _hasShiftTransferPermission = false;
                _hasAssignBarcodePermission = false;
                _hasManageRecipesPermission = false;
                _hasDisposalPermission = false;
                _hasWriteOffPermission = false;
                _hasSendToSalePermission = false;
                UpdateCardVisibility();
                return;
            }

            try
            {
                var activeReports = await _apiService.GetActiveWorkReportsAsync(currentUser.Id);
                _hasActiveShift = activeReports.Any();
                _isPrivilegedUser = IsPrivilegedUser(currentUser);

                // Загружаем все права параллельно
                var shiftTask = CheckPermissionAsync(currentUser, "ShiftTransfer");
                var barcodeTask = CheckPermissionAsync(currentUser, "AssignBarcode");
                var recipesTask = CheckPermissionAsync(currentUser, "ManageRecipes");
                var scrapTask = CheckPermissionAsync(currentUser, "SendToScrap");
                var writeOffTask = CheckPermissionAsync(currentUser, "WriteOff");
                var sendToSaleTask = CheckPermissionAsync(currentUser, "SendToSale");
                var finishedGoodsTask = CheckPermissionAsync(currentUser, "ManageFinishedGoodsWarehouses");

                await Task.WhenAll(shiftTask, barcodeTask, recipesTask, scrapTask, writeOffTask, sendToSaleTask, finishedGoodsTask);

                _hasShiftTransferPermission = shiftTask.Result;
                _hasAssignBarcodePermission = barcodeTask.Result;
                _hasManageRecipesPermission = recipesTask.Result;
                _hasWriteOffPermission = writeOffTask.Result;
                _hasSendToSalePermission = sendToSaleTask.Result;
                _hasDisposalPermission = scrapTask.Result || writeOffTask.Result;
                _hasManageFinishedGoodsPermission = finishedGoodsTask.Result;

                UpdateCardVisibility();
            }
            catch
            {
                _hasActiveShift = false;
                _hasShiftTransferPermission = false;
                _hasAssignBarcodePermission = false;
                _hasManageRecipesPermission = false;
                _hasDisposalPermission = false;
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

        private void UpdateCardVisibility()
        {
            // Сканер - только с правом AssignBarcode
            ScannerCard.IsVisible = _hasAssignBarcodePermission || _isPrivilegedUser;

            // Отчеты - скрываем если есть право на передачу смены (кроме привилегированных)
            WorkReportsCard.IsVisible = !_hasShiftTransferPermission || _isPrivilegedUser;

            // Передача смены - только с правом ShiftTransfer
            ShiftTransfersCard.IsVisible = _hasShiftTransferPermission || _isPrivilegedUser;

            // Производство - только с правом ManageRecipes
            ReprocessingCard.IsVisible = _hasManageRecipesPermission || _isPrivilegedUser;

            // Утиль - только с правом WriteOff
            DisposalCard.IsVisible = _hasWriteOffPermission || _isPrivilegedUser;

            // Упаковка - только с правом SendToSale
            ProductOutputCard.IsVisible = _hasSendToSalePermission || _isPrivilegedUser;

            // Готовая продукция - только с правом ManageFinishedGoodsWarehouses
            FinishedGoodsCard.IsVisible = _hasManageFinishedGoodsPermission || _isPrivilegedUser;
        }

        private async Task<bool> EnsureShiftAccessAsync(string destination)
        {
            await RefreshShiftStateAsync();

            if (_hasActiveShift)
            {
                return true;
            }

            if (destination == "ShiftTransfersPage")
            {
                return true;
            }

            if (destination == "WorkReportsPage" && !_hasShiftTransferPermission)
            {
                return true;
            }

            if (destination == "WorkReportsPage" && _isPrivilegedUser)
            {
                return true;
            }

            var message = _hasShiftTransferPermission && !_isPrivilegedUser
                ? "Смена не начата. Доступна только передача смены."
                : "Смена не начата. Доступно только начало смены или передача смены.";
            await DisplayAlert("Смена не начата", message, "OK");
            return false;
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

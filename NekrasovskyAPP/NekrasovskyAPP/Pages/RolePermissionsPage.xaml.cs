using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;
using Microsoft.Maui.ApplicationModel;

namespace NekrasovskyAPP.Pages
{
    public partial class RolePermissionsPage : ContentPage
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        private List<Role> _roles = new();
        private List<PermissionViewModel> _permissions = new();
        private Role? _selectedRole;

        public List<PermissionViewModel> Permissions => _permissions;

        public RolePermissionsPage(IApiService apiService, IAuthService authService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Проверка что пользователь - Owner
            if (!IsCurrentUserOwner())
            {
                await DisplayAlert("Доступ запрещён", "Только владелец может управлять правами ролей", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            await LoadRolesAsync();
            SetPickerHeight();
        }

        private void SetPickerHeight()
        {
            // Устанавливаем высоту 58 для Picker на ПК
            if (IsDesktop() && RolePickerBorder != null)
            {
                RolePickerBorder.HeightRequest = 58.0;
            }
            else if (RolePickerBorder != null)
            {
                RolePickerBorder.HeightRequest = 48.0;
            }
        }

        private bool IsDesktop()
        {
            return DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Platform == DevicePlatform.WinUI;
        }

        private bool IsCurrentUserOwner()
        {
            var user = _authService.CurrentUser;
            if (user?.Role == null)
                return false;

            return user.Role.Code?.Equals("Owner", StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                ShowLoading(true);
                _roles = await _apiService.GetAllRolesAsync();
                RolePicker.ItemsSource = _roles;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить роли: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnRoleSelected(object sender, EventArgs e)
        {
            if (RolePicker.SelectedItem is not Role selectedRole)
                return;

            _selectedRole = selectedRole;

            // Показать информацию о роли
            RoleNameLabel.Text = selectedRole.Name;
            RoleCodeLabel.Text = $"Код: {selectedRole.Code}";
            RoleDescriptionLabel.Text = selectedRole.Description ?? "Без описания";
            RoleInfoContainer.IsVisible = true;

            await LoadRolePermissionsAsync(selectedRole.Id);
        }

        private async Task LoadRolePermissionsAsync(int roleId)
        {
            try
            {
                ShowLoading(true);

                // Загрузить все права и права роли параллельно
                var allPermissionsTask = _apiService.GetAllPermissionsAsync();
                var rolePermissionsTask = _apiService.GetRolePermissionsAsync(roleId);

                await Task.WhenAll(allPermissionsTask, rolePermissionsTask);

                var allPermissions = await allPermissionsTask;
                var rolePermissions = await rolePermissionsTask;
                var rolePermissionIds = rolePermissions.Select(p => p.Id).ToHashSet();

                // Создать ViewModel для каждого права
                _permissions = allPermissions.Select(p => new PermissionViewModel
                {
                    Permission = p,
                    IsSelected = rolePermissionIds.Contains(p.Id)
                }).ToList();

                // Обновляем BindableLayout
                BindableLayout.SetItemsSource(PermissionsList, null);
                BindableLayout.SetItemsSource(PermissionsList, _permissions);

                PermissionsContainer.IsVisible = true;
                SaveButton.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить права: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_selectedRole == null)
            {
                await DisplayAlert("Ошибка", "Сначала выберите роль", "OK");
                return;
            }

            try
            {
                ShowLoading(true);
                SaveButton.IsEnabled = false;

                var selectedPermissionIds = _permissions
                    .Where(p => p.IsSelected)
                    .Select(p => p.Permission.Id)
                    .ToList();

                var success = await _apiService.UpdateRolePermissionsAsync(_selectedRole.Id, selectedPermissionIds);

                if (success)
                {
                    await DisplayAlert("Успех", $"Права роли \"{_selectedRole.Name}\" обновлены.\nВыбрано прав: {selectedPermissionIds.Count}", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось обновить права роли. Возможно, у вас нет прав для этого действия.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
                SaveButton.IsEnabled = true;
            }
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
        }
    }
}

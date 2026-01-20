using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Linq;

namespace NekrasovskyAPP.Pages
{
    public partial class UsersPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private string _lastSearchText = string.Empty;

        public UsersPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadUsersAsync();
            // Загружаем роли при открытии страницы, если еще не загружены
            if (!_viewModel.Roles.Any())
            {
                await _viewModel.LoadRolesAsync();
            }
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadUsersAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadUsersAsync();
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowUserDialogAsync(null);
        }

        private async void OnUserSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is User selectedUser)
            {
                // Показываем меню действий при выборе пользователя
                var action = await DisplayActionSheet(
                    $"Пользователь: {selectedUser.Name} {selectedUser.Surname}",
                    "Отмена",
                    null,
                    "Просмотр",
                    "Редактировать",
                    "Удалить");

                switch (action)
                {
                    case "Просмотр":
                        await DisplayAlert("Информация о пользователе",
                            $"Имя: {selectedUser.Name}\n" +
                            $"Фамилия: {selectedUser.Surname}\n" +
                            $"Email: {selectedUser.Email}\n" +
                            $"Логин: {selectedUser.Login}\n" +
                            $"Телефон: {selectedUser.Phone ?? "Не указан"}\n" +
                            $"Роль: {selectedUser.Role?.Name ?? "Не назначена"}\n" +
                            $"Дата создания: {selectedUser.CreatedAt:dd.MM.yyyy HH:mm}",
                            "OK");
                        break;

                    case "Редактировать":
                        await ShowUserDialogAsync(selectedUser);
                        break;

                    case "Удалить":
                        var confirm = await DisplayAlert(
                            "Подтверждение удаления",
                            $"Вы уверены, что хотите удалить пользователя {selectedUser.Name} {selectedUser.Surname}?",
                            "Удалить",
                            "Отмена");

                        if (confirm)
                        {
                            var success = await _viewModel.DeleteUserAsync(selectedUser.Id);
                            if (!success)
                            {
                                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                            }
                            else
                            {
                                await DisplayAlert("Успех", "Пользователь успешно удален", "OK");
                            }
                        }
                        break;
                }

                // Снимаем выделение
                UsersCollectionView.SelectedItem = null;
            }
        }

        private async Task ShowUserDialogAsync(User? existingUser)
        {
            bool isEdit = existingUser != null;
            string title = isEdit ? "Редактирование пользователя" : "Создание пользователя";

            // Запрашиваем данные
            var login = await DisplayPromptAsync(title, "Логин:", "Сохранить", "Отмена", "Логин", -1, Keyboard.Default, existingUser?.Login ?? "");
            if (string.IsNullOrWhiteSpace(login))
                return;

            var password = isEdit ? null : await DisplayPromptAsync(title, "Пароль:", "Далее", "Отмена", "Пароль", -1, Keyboard.Default);
            if (!isEdit && string.IsNullOrWhiteSpace(password))
                return;

            var name = await DisplayPromptAsync(title, "Имя:", "Далее", "Отмена", "Имя", -1, Keyboard.Default, existingUser?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var surname = await DisplayPromptAsync(title, "Фамилия:", "Далее", "Отмена", "Фамилия", -1, Keyboard.Default, existingUser?.Surname ?? "");
            if (string.IsNullOrWhiteSpace(surname))
                return;

            var email = await DisplayPromptAsync(title, "Email:", "Далее", "Отмена", "Email", -1, Keyboard.Email, existingUser?.Email ?? "");
            if (string.IsNullOrWhiteSpace(email))
                return;

            var phone = await DisplayPromptAsync(title, "Телефон (необязательно):", "Далее", "Отмена", "Телефон", -1, Keyboard.Telephone, existingUser?.Phone ?? "");

            // Загружаем роли, если еще не загружены
            if (!_viewModel.Roles.Any())
            {
                await _viewModel.LoadRolesAsync();
            }

            // Выбор роли
            int? selectedRoleId = existingUser?.RoleId;
            if (_viewModel.Roles.Any())
            {
                var roleOptions = new List<string> { "Без роли" };
                roleOptions.AddRange(_viewModel.Roles.Select(r => r.Name));
                
                // Определяем текущую выбранную роль для отображения
                var currentRoleName = existingUser?.Role?.Name ?? "Без роли";
                
                var selectedRoleIndex = await DisplayActionSheet(
                    "Выберите роль:",
                    "Отмена",
                    null,
                    roleOptions.ToArray());

                if (selectedRoleIndex == "Отмена")
                {
                    return; // Пользователь отменил операцию
                }
                else if (selectedRoleIndex == "Без роли" || string.IsNullOrEmpty(selectedRoleIndex))
                {
                    selectedRoleId = null;
                }
                else if (!string.IsNullOrEmpty(selectedRoleIndex))
                {
                    var selectedRole = _viewModel.Roles.FirstOrDefault(r => r.Name == selectedRoleIndex);
                    if (selectedRole != null)
                    {
                        selectedRoleId = selectedRole.Id;
                    }
                }
            }

            // Создаем или обновляем пользователя
            var user = existingUser ?? new User();
            user.Login = login;
            user.Name = name;
            user.Surname = surname;
            user.Email = email;
            user.Phone = phone ?? string.Empty;
            user.RoleId = selectedRoleId;

            if (!isEdit && !string.IsNullOrWhiteSpace(password))
            {
                user.EncryptedPassword = password;
            }
            else if (isEdit)
            {
                // При редактировании пароль не меняем, если не указан новый
                var newPassword = await DisplayPromptAsync(title, "Новый пароль (оставьте пустым, чтобы не менять):", "Сохранить", "Пропустить", "Пароль", -1, Keyboard.Default);
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    user.EncryptedPassword = newPassword;
                }
            }

            bool success;
            if (isEdit)
            {
                success = await _viewModel.UpdateUserAsync(existingUser!.Id, user);
            }
            else
            {
                success = await _viewModel.CreateUserAsync(user);
            }

            if (success)
            {
                await DisplayAlert("Успех", isEdit ? "Пользователь успешно обновлен" : "Пользователь успешно создан", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue ?? string.Empty;
            
            // Debounce search
            if (searchText == _lastSearchText)
                return;
            
            _lastSearchText = searchText;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await _viewModel.LoadUsersAsync();
            }
            else
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = parts.Length > 0 ? parts[0] : null;
                var surname = parts.Length > 1 ? parts[1] : null;
                
                await _viewModel.SearchUsersAsync(name, surname);
            }
        }
    }
}


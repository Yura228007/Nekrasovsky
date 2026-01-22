using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly IApiService _apiService;

        public SettingsPage(IApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSystemInfoAsync();
        }

        private async Task LoadSystemInfoAsync()
        {
            try
            {
                // Проверка подключения к БД через API
                var response = await _apiService.CheckDatabaseAsync();
                if (response != null && response.ContainsKey("success") && 
                    response["success"].ToString()?.ToLower() == "true")
                {
                    var usersCount = response.ContainsKey("usersCount") ? response["usersCount"]?.ToString() ?? "?" : "?";
                    DatabaseStatusLabel.Text = $"✅ База данных подключена\n" +
                                              $"Пользователей в системе: {usersCount}";
                    DatabaseStatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    string message = "Неизвестная ошибка";
                    if (response != null && response.ContainsKey("message") && response["message"] != null)
                    {
                        var messageValue = response["message"];
                        message = messageValue?.ToString() ?? "Неизвестная ошибка";
                    }
                    DatabaseStatusLabel.Text = $"❌ Ошибка подключения к базе данных\n{message}";
                    DatabaseStatusLabel.TextColor = Colors.Red;
                }

                // Информация о системе
                SystemInfoLabel.Text = $"Версия приложения: 1.0.0\n" +
                                      $"Платформа: {DeviceInfo.Platform}\n" +
                                      $"Версия ОС: {DeviceInfo.VersionString}\n" +
                                      $"Модель: {DeviceInfo.Model ?? "Неизвестно"}";

                // URL API (из ApiService)
                ApiUrlLabel.Text = "URL API можно изменить в ApiService.cs\n" +
                                  "Для Android: обновите метод GetBaseUrl()";
            }
            catch (Exception ex)
            {
                DatabaseStatusLabel.Text = $"❌ Ошибка: {ex.Message}";
                DatabaseStatusLabel.TextColor = Colors.Red;
                SystemInfoLabel.Text = $"Не удалось загрузить информацию о системе\nОшибка: {ex.Message}";
            }
        }

        private async void OnCheckDatabaseClicked(object sender, EventArgs e)
        {
            await LoadSystemInfoAsync();
            await DisplayAlert("Проверка завершена", "Информация обновлена", "OK");
        }

        private async void OnRefreshInfoClicked(object sender, EventArgs e)
        {
            await LoadSystemInfoAsync();
            await DisplayAlert("Информация обновлена", "Системная информация обновлена", "OK");
        }

    }
}

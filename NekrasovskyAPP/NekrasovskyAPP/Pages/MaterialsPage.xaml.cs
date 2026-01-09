using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class MaterialsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MaterialsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadMaterialsAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadMaterialsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadMaterialsAsync();
        }

        private string _lastSearchText = string.Empty;

        private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue ?? string.Empty;
            
            if (searchText == _lastSearchText)
                return;
            
            _lastSearchText = searchText;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await _viewModel.LoadMaterialsAsync();
            }
            else
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = parts.Length > 0 ? parts[0] : null;
                var code = parts.Length > 1 ? parts[1] : null;
                
                await _viewModel.SearchMaterialsAsync(name, code);
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowMaterialDialogAsync(null);
        }

        private async void OnMaterialSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Material selectedMaterial)
            {
                var action = await DisplayActionSheet(
                    $"Материал: {selectedMaterial.Name}",
                    "Отмена",
                    null,
                    "Просмотр",
                    "Редактировать",
                    "Удалить");

                switch (action)
                {
                    case "Просмотр":
                        await DisplayAlert("Информация о материале",
                            $"Название: {selectedMaterial.Name}\n" +
                            $"Код: {selectedMaterial.Code ?? "Не указан"}\n" +
                            $"Описание: {selectedMaterial.Description ?? "Не указано"}\n" +
                            $"Единица измерения: {selectedMaterial.MeasuringUnit}",
                            "OK");
                        break;

                    case "Редактировать":
                        await ShowMaterialDialogAsync(selectedMaterial);
                        break;

                    case "Удалить":
                        var confirm = await DisplayAlert(
                            "Подтверждение удаления",
                            $"Вы уверены, что хотите удалить материал {selectedMaterial.Name}?",
                            "Удалить",
                            "Отмена");

                        if (confirm)
                        {
                            var success = await _viewModel.DeleteMaterialAsync(selectedMaterial.Id);
                            if (!success)
                            {
                                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                            }
                            else
                            {
                                await DisplayAlert("Успех", "Материал успешно удален", "OK");
                            }
                        }
                        break;
                }

                MaterialsCollectionView.SelectedItem = null;
            }
        }

        private async Task ShowMaterialDialogAsync(Material? existingMaterial)
        {
            bool isEdit = existingMaterial != null;
            string title = isEdit ? "Редактирование материала" : "Создание материала";

            var name = await DisplayPromptAsync(title, "Название:", "Далее", "Отмена", "Название", -1, Keyboard.Default, existingMaterial?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var code = await DisplayPromptAsync(title, "Код (необязательно):", "Далее", "Отмена", "Код", -1, Keyboard.Default, existingMaterial?.Code ?? "");
            
            var description = await DisplayPromptAsync(title, "Описание (необязательно):", "Далее", "Отмена", "Описание", -1, Keyboard.Default, existingMaterial?.Description ?? "");
            
            var measuringUnit = await DisplayPromptAsync(title, "Единица измерения (шт, кг, л и т.д.):", "Сохранить", "Отмена", "Единица измерения", -1, Keyboard.Default, existingMaterial?.MeasuringUnit ?? "шт");
            if (string.IsNullOrWhiteSpace(measuringUnit))
                measuringUnit = "шт";

            var material = existingMaterial ?? new Material();
            material.Name = name;
            material.Code = code;
            material.Description = description;
            material.MeasuringUnit = measuringUnit;

            bool success;
            if (isEdit)
            {
                success = await _viewModel.UpdateMaterialAsync(existingMaterial!.Id, material);
            }
            else
            {
                success = await _viewModel.CreateMaterialAsync(material);
            }

            if (success)
            {
                await DisplayAlert("Успех", isEdit ? "Материал успешно обновлен" : "Материал успешно создан", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }
    }
}


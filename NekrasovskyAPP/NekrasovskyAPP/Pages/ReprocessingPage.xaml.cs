using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class ReprocessingPage : ContentPage
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly List<Warehouse> _warehouses = new();
        private readonly List<Material> _materials = new();
        private readonly List<Product> _products = new();
        private readonly List<Material> _responsibleMaterials = new();

        public ReprocessingPage(IApiService apiService, IAuthService authService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            _warehouses.Clear();
            _materials.Clear();
            _products.Clear();
            _responsibleMaterials.Clear();

            _warehouses.AddRange(await _apiService.GetAllWarehousesAsync());
            _materials.AddRange(await _apiService.GetAllMaterialsAsync());
            _products.AddRange(await _apiService.GetAllProductsAsync());

            var responsibilities = await _apiService.GetResponsibilitiesByUserAsync(user.Id, true);
            var materialIds = responsibilities
                .Where(r => r.MaterialId.HasValue)
                .Select(r => r.MaterialId!.Value)
                .ToHashSet();

            foreach (var material in _materials.Where(m => materialIds.Contains(m.Id)))
            {
                _responsibleMaterials.Add(material);
            }

            WarehousePicker.ItemsSource = _warehouses;
            SourceMaterialPicker.ItemsSource = _responsibleMaterials;

            Target1TypePicker.ItemsSource = new List<string> { "Материал", "Продукт" };
            Target2TypePicker.ItemsSource = new List<string> { "Материал", "Продукт" };
        }

        private void OnTarget1TypeChanged(object? sender, EventArgs e)
        {
            UpdateTargetPicker(Target1TypePicker, Target1Picker);
        }

        private void OnTarget2TypeChanged(object? sender, EventArgs e)
        {
            UpdateTargetPicker(Target2TypePicker, Target2Picker);
        }

        private void UpdateTargetPicker(Picker typePicker, Picker targetPicker)
        {
            if (typePicker.SelectedItem?.ToString() == "Материал")
            {
                targetPicker.ItemsSource = _materials;
                targetPicker.ItemDisplayBinding = new Binding(nameof(Material.Name));
            }
            else if (typePicker.SelectedItem?.ToString() == "Продукт")
            {
                targetPicker.ItemsSource = _products;
                targetPicker.ItemDisplayBinding = new Binding(nameof(Product.Name));
            }
            else
            {
                targetPicker.ItemsSource = null;
            }
        }

        private void OnSecondOutputToggled(object? sender, ToggledEventArgs e)
        {
            SecondOutputBlock.IsVisible = e.Value;
        }

        private async void OnSubmitClicked(object? sender, EventArgs e)
        {
            if (WarehousePicker.SelectedItem is not Warehouse warehouse)
            {
                await DisplayAlert("Ошибка", "Выберите склад", "OK");
                return;
            }

            if (SourceMaterialPicker.SelectedItem is not Material sourceMaterial)
            {
                await DisplayAlert("Ошибка", "Выберите исходный материал", "OK");
                return;
            }

            if (!int.TryParse(SourceQuantityEntry.Text, out var sourceQuantity) || sourceQuantity <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество переработки", "OK");
                return;
            }

            var outputs = new List<ReprocessingOutput>();

            var output1 = BuildOutput(Target1TypePicker, Target1Picker, Target1QuantityEntry);
            if (output1 == null)
            {
                await DisplayAlert("Ошибка", "Заполните результат 1", "OK");
                return;
            }
            outputs.Add(output1);

            if (SecondOutputSwitch.IsToggled)
            {
                var output2 = BuildOutput(Target2TypePicker, Target2Picker, Target2QuantityEntry);
                if (output2 == null)
                {
                    await DisplayAlert("Ошибка", "Заполните результат 2", "OK");
                    return;
                }
                outputs.Add(output2);
            }

            var request = new ReprocessingCreateRequest
            {
                WarehouseId = warehouse.Id,
                SourceMaterialId = sourceMaterial.Id,
                SourceQuantity = sourceQuantity,
                Outputs = outputs
            };

            var response = await _apiService.CreateReprocessingAsync(request);
            if (response.GetData() == null)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Не удалось выполнить переработку", "OK");
                return;
            }

            await DisplayAlert("Успех", "Переработка выполнена", "OK");
            ClearForm();
        }

        private ReprocessingOutput? BuildOutput(Picker typePicker, Picker targetPicker, Entry quantityEntry)
        {
            if (!int.TryParse(quantityEntry.Text, out var quantity) || quantity <= 0)
            {
                return null;
            }

            if (typePicker.SelectedItem?.ToString() == "Материал" && targetPicker.SelectedItem is Material material)
            {
                return new ReprocessingOutput
                {
                    MaterialId = material.Id,
                    Quantity = quantity,
                    MeasuringType = material.MeasuringUnit
                };
            }

            if (typePicker.SelectedItem?.ToString() == "Продукт" && targetPicker.SelectedItem is Product product)
            {
                return new ReprocessingOutput
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    MeasuringType = product.MeasuringUnit
                };
            }

            return null;
        }

        private void ClearForm()
        {
            SourceQuantityEntry.Text = string.Empty;
            Target1QuantityEntry.Text = string.Empty;
            Target2QuantityEntry.Text = string.Empty;
            Target1TypePicker.SelectedItem = null;
            Target2TypePicker.SelectedItem = null;
            Target1Picker.ItemsSource = null;
            Target2Picker.ItemsSource = null;
            SecondOutputSwitch.IsToggled = false;
        }
    }
}

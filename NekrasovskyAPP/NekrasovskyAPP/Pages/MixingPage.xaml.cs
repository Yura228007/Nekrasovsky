using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NekrasovskyAPP.Pages
{
    public partial class MixingPage : ContentPage
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly List<Warehouse> _warehouses = new();
        private readonly List<Material> _materials = new();
        private readonly List<Material> _responsibleMaterials = new();
        private List<ResponsibilityStockItem> _responsibilityStock = new();
        private Warehouse? _selectedWarehouse;

        public ObservableCollection<MixingSourceItem> Sources { get; } = new();
        public ICommand AddSourceCommand { get; }
        public ICommand RemoveSourceCommand { get; }

        public MixingPage(IApiService apiService, IAuthService authService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            AddSourceCommand = new Command(AddSource);
            RemoveSourceCommand = new Command<MixingSourceItem>(RemoveSource);
            BindingContext = this;

            // Инициализируем единицы измерения
            MeasuringUnitPicker.ItemsSource = new List<string> { "шт", "кг", "г", "л", "мл", "м", "см" };
            MeasuringUnitPicker.SelectedItem = "шт";

            // Добавляем первый исходный материал
            Sources.Add(new MixingSourceItem(new List<Material>(), IsDesktop()));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
            SetPickerHeights();
        }

        private void SetPickerHeights()
        {
            var height = IsDesktop() ? 58.0 : 48.0;
            if (WarehousePickerBorder != null)
                WarehousePickerBorder.HeightRequest = height;
            if (MeasuringUnitPickerBorder != null)
                MeasuringUnitPickerBorder.HeightRequest = height;
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
            _responsibleMaterials.Clear();
            _responsibilityStock.Clear();

            _warehouses.AddRange((await _apiService.GetAllWarehousesAsync())
                .Where(w => w.IsActive));
            _materials.AddRange(await _apiService.GetAllMaterialsAsync());

            // Остатки под ответственностью пользователя
            _responsibilityStock = await _apiService.GetResponsibilityStockAsync(user.Id);
            var materialIdsFromStock = _responsibilityStock
                .Where(s => s.ItemType == "Material")
                .Select(s => s.ItemId)
                .ToHashSet();
            foreach (var material in _materials.Where(m => materialIdsFromStock.Contains(m.Id)))
            {
                _responsibleMaterials.Add(material);
            }

            WarehousePicker.ItemsSource = _warehouses;
            UpdateSourceMaterials();
        }

        private void OnWarehouseSelected(object? sender, EventArgs e)
        {
            _selectedWarehouse = WarehousePicker.SelectedItem as Warehouse;
            UpdateSourceMaterials();
        }

        private void UpdateSourceMaterials()
        {
            var materialsList = GetAvailableMaterials();
            foreach (var source in Sources)
            {
                source.UpdateAvailableMaterials(materialsList);
            }
        }

        private void AddSource()
        {
            var availableMaterials = GetAvailableMaterials();
            Sources.Add(new MixingSourceItem(availableMaterials, IsDesktop()));
        }

        private List<Material> GetAvailableMaterials()
        {
            if (_selectedWarehouse != null)
            {
                var materialIdsAtWarehouse = _responsibilityStock
                    .Where(s => s.ItemType == "Material" && s.Warehouses != null)
                    .Where(s => s.Warehouses!.Any(w => w.WarehouseId == _selectedWarehouse!.Id && w.Quantity > 0))
                    .Select(s => s.ItemId)
                    .ToHashSet();
                return _materials.Where(m => materialIdsAtWarehouse.Contains(m.Id)).ToList();
            }
            return _responsibleMaterials.ToList();
        }

        private bool IsDesktop()
        {
            return DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Platform == DevicePlatform.WinUI;
        }

        private void RemoveSource(MixingSourceItem? item)
        {
            if (item == null || Sources.Count <= 1)
                return;
            Sources.Remove(item);
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await LoadDataAsync();
            SetPickerHeights();
        }

        private async void OnScanCodeClicked(object? sender, EventArgs e)
        {
#if ANDROID || IOS
            try
            {
                var scannerPage = new BarcodeScannerPage();
                scannerPage.BarcodeScanned += (s, code) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OutputCodeEntry.Text = code;
                    });
                };
                await Navigation.PushModalAsync(scannerPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть сканер: {ex.Message}", "OK");
            }
#else
            await DisplayAlert("Недоступно", "Сканирование доступно только на мобильных устройствах", "OK");
#endif
        }

        private async void OnSubmitClicked(object? sender, EventArgs e)
        {
            if (WarehousePicker.SelectedItem is not Warehouse warehouse)
            {
                await DisplayAlert("Ошибка", "Выберите склад", "OK");
                return;
            }

            // Проверяем исходные материалы
            var sources = new List<ReprocessingSource>();
            foreach (var sourceItem in Sources)
            {
                if (!TryBuildSource(sourceItem, out var source))
                {
                    await DisplayAlert("Ошибка", "Заполните все исходные материалы корректно", "OK");
                    return;
                }
                sources.Add(source);
            }

            // Проверяем результат
            if (string.IsNullOrWhiteSpace(OutputCodeEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите код результирующего материала", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputNameEntry.Text))
            {
                await DisplayAlert("Ошибка", "Введите название результирующего материала", "OK");
                return;
            }

            if (!int.TryParse(OutputQuantityEntry.Text, out var outputQuantity) || outputQuantity <= 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное количество результата", "OK");
                return;
            }

            var measuringUnit = MeasuringUnitPicker.SelectedItem as string ?? "шт";

            var output = new ReprocessingOutput
            {
                MaterialId = null,
                NewMaterialCode = OutputCodeEntry.Text.Trim(),
                NewMaterialName = OutputNameEntry.Text.Trim(),
                ProductId = null,
                Quantity = outputQuantity,
                MeasuringType = measuringUnit
            };

            // Получаем количество брака
            var defectQuantity = 0;
            if (int.TryParse(DefectQuantityEntry.Text, out var parsedDefect) && parsedDefect > 0)
            {
                defectQuantity = parsedDefect;
            }

            var request = new ReprocessingCreateRequest
            {
                WarehouseId = warehouse.Id,
                Sources = sources,
                Outputs = new List<ReprocessingOutput> { output },
                DefectQuantity = defectQuantity
            };

            var response = await _apiService.CreateReprocessingAsync(request);
            if (response.GetData() == null)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Не удалось выполнить смешивание", "OK");
                return;
            }

            await DisplayAlert("Успех", "Смешивание выполнено", "OK");
            ClearForm();
        }

        private void ClearForm()
        {
            Sources.Clear();
            var availableMaterials = GetAvailableMaterials();
            Sources.Add(new MixingSourceItem(availableMaterials, IsDesktop()));
            WarehousePicker.SelectedItem = null;
            _selectedWarehouse = null;
            OutputCodeEntry.Text = string.Empty;
            OutputNameEntry.Text = string.Empty;
            OutputQuantityEntry.Text = string.Empty;
            MeasuringUnitPicker.SelectedItem = "шт";
            DefectQuantityEntry.Text = "0";
            NoteEditor.Text = string.Empty;
        }

        private static bool TryBuildSource(MixingSourceItem sourceItem, out ReprocessingSource source)
        {
            source = new ReprocessingSource();

            if (sourceItem.SelectedMaterial is not Material material)
                return false;

            if (!int.TryParse(sourceItem.Quantity, out var quantity) || quantity <= 0)
                return false;

            source.MaterialId = material.Id;
            source.Quantity = quantity;
            source.MeasuringType = material.MeasuringUnit;
            return true;
        }

        public class MixingSourceItem : INotifyPropertyChanged
        {
            private readonly List<Material> _availableMaterials;
            private readonly bool _isDesktop;
            private Material? _selectedMaterial;
            private string? _quantity;
            private IList<Material> _materialItems = new List<Material>();

            public MixingSourceItem(List<Material> availableMaterials, bool isDesktop)
            {
                _availableMaterials = availableMaterials;
                _isDesktop = isDesktop;
                RefreshMaterials();
            }

            public IList<Material> Materials
            {
                get => _materialItems;
                private set
                {
                    _materialItems = value;
                    OnPropertyChanged();
                }
            }

            public Material? SelectedMaterial
            {
                get => _selectedMaterial;
                set
                {
                    if (_selectedMaterial == value)
                        return;
                    _selectedMaterial = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MeasuringUnit));
                }
            }

            public string MeasuringUnit => _selectedMaterial?.MeasuringUnit ?? "";

            public string? Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity == value)
                        return;
                    _quantity = value;
                    OnPropertyChanged();
                }
            }

            public double PickerHeight => _isDesktop ? 58.0 : 48.0;

            public void RefreshMaterials()
            {
                Materials = _availableMaterials.ToList();
                SelectedMaterial = null;
            }

            public void UpdateAvailableMaterials(List<Material> newMaterials)
            {
                _availableMaterials.Clear();
                _availableMaterials.AddRange(newMaterials);
                RefreshMaterials();
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

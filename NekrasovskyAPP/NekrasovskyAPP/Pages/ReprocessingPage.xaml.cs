using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

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
        private readonly List<FillingWarehouse> _fillingWarehouses = new();
        private List<ResponsibilityStockItem> _responsibilityStock = new();
        private Warehouse? _selectedWarehouse;
        private List<Warehouse> _disposalWarehouses = new();
        private Warehouse? _selectedDefectWarehouse;
        private Warehouse? _selectedRecyclingWarehouse;

        public ObservableCollection<SourceItem> Sources { get; } = new();
        public ObservableCollection<OutputItem> NormalOutputs { get; } = new();
        
        // Для обратной совместимости (используется в XAML)
        public ObservableCollection<OutputItem> Outputs => NormalOutputs;
        public ICommand AddSourceCommand { get; }
        public ICommand RemoveSourceCommand { get; }
        public ICommand AddOutputCommand { get; }
        public ICommand RemoveOutputCommand { get; }
        public ICommand ScanMaterialCodeCommand { get; }

        public ReprocessingPage(IApiService apiService, IAuthService authService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            AddSourceCommand = new Command(AddSource);
            RemoveSourceCommand = new Command<SourceItem>(RemoveSource);
            AddOutputCommand = new Command(AddOutput);
            RemoveOutputCommand = new Command<OutputItem>(RemoveOutput);
            ScanMaterialCodeCommand = new Command<OutputItem>(ScanMaterialCode);
            BindingContext = this;
            // Инициализируем с пустым списком, данные загрузятся в OnAppearing
            Sources.Add(new SourceItem(new List<Material>(), IsDesktop()));
            NormalOutputs.Add(new OutputItem(_products) { OutputType = ReprocessingOutputType.Normal });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
            SetPickerHeights();
        }

        private void SetPickerHeights()
        {
            // Устанавливаем высоту 58 для Picker на ПК
            if (IsDesktop())
            {
                if (WarehousePickerBorder != null)
                    WarehousePickerBorder.HeightRequest = 58.0;
                if (DefectWarehousePickerBorder != null)
                    DefectWarehousePickerBorder.HeightRequest = 58.0;
                if (RecyclingWarehousePickerBorder != null)
                    RecyclingWarehousePickerBorder.HeightRequest = 58.0;
            }
            else
            {
                if (WarehousePickerBorder != null)
                    WarehousePickerBorder.HeightRequest = 48.0;
                if (DefectWarehousePickerBorder != null)
                    DefectWarehousePickerBorder.HeightRequest = 48.0;
                if (RecyclingWarehousePickerBorder != null)
                    RecyclingWarehousePickerBorder.HeightRequest = 48.0;
            }
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
            _fillingWarehouses.Clear();
            _responsibilityStock.Clear();

            _warehouses.AddRange((await _apiService.GetAllWarehousesAsync())
                .Where(w => w.IsActive));
            _materials.AddRange(await _apiService.GetAllMaterialsAsync());
            _products.AddRange(await _apiService.GetAllProductsAsync());
            _fillingWarehouses.AddRange(await _apiService.GetAllFillingWarehousesAsync());
            
            // Загружаем склады утиля
            _disposalWarehouses = _warehouses
                .Where(w => w.Type != null && w.Type.Equals("Утиль", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Остатки под ответственностью пользователя — из ResponsibilityFilling (то же, что при передаче смены)
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
            
            // Инициализируем Picker'ы для складов утиля
            DefectWarehousePicker.ItemsSource = _disposalWarehouses;
            RecyclingWarehousePicker.ItemsSource = _disposalWarehouses;
            
            // Устанавливаем высоту Picker'ов
            if (IsDesktop())
            {
                DefectWarehousePickerBorder.HeightRequest = 58.0;
                RecyclingWarehousePickerBorder.HeightRequest = 58.0;
            }
            else
            {
                DefectWarehousePickerBorder.HeightRequest = 48.0;
                RecyclingWarehousePickerBorder.HeightRequest = 48.0;
            }
            
            // Обновляем материалы для исходников после загрузки данных
            UpdateSourceMaterials();
            
            // Обновляем продукты для результатов
            foreach (var output in NormalOutputs)
            {
                output.UpdateProducts(_products);
            }
        }

        private void OnWarehouseSelected(object? sender, EventArgs e)
        {
            _selectedWarehouse = WarehousePicker.SelectedItem as Warehouse;
            UpdateSourceMaterials();
        }

        private void OnDefectWarehouseSelected(object? sender, EventArgs e)
        {
            _selectedDefectWarehouse = DefectWarehousePicker.SelectedItem as Warehouse;
        }

        private void OnRecyclingWarehouseSelected(object? sender, EventArgs e)
        {
            _selectedRecyclingWarehouse = RecyclingWarehousePicker.SelectedItem as Warehouse;
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
            Sources.Add(new SourceItem(availableMaterials, IsDesktop()));
        }

        private List<Material> GetAvailableMaterials()
        {
            // Материалы под ответственностью пользователя на выбранном складе (из ResponsibilityFilling)
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

        private void RemoveSource(SourceItem? item)
        {
            if (item == null || Sources.Count <= 1)
            {
                return;
            }

            Sources.Remove(item);
        }

        private void AddOutput()
        {
            NormalOutputs.Add(new OutputItem(_products) { OutputType = ReprocessingOutputType.Normal });
        }

        private async void ScanMaterialCode(OutputItem? outputItem)
        {
            if (outputItem == null) return;

#if ANDROID || IOS
            try
            {
                var scannerPage = new BarcodeScannerPage();
                scannerPage.BarcodeScanned += (s, code) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        outputItem.MaterialCode = code;
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

        private void RemoveOutput(OutputItem? item)
        {
            if (item == null || NormalOutputs.Count <= 1)
            {
                return;
            }
            NormalOutputs.Remove(item);
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await LoadDataAsync();
            SetPickerHeights();
        }

        private async void OnSubmitClicked(object? sender, EventArgs e)
        {
            if (WarehousePicker.SelectedItem is not Warehouse warehouse)
            {
                await DisplayAlert("Ошибка", "Выберите склад", "OK");
                return;
            }

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

            var outputs = new List<ReprocessingOutput>();

            // Обрабатываем нормальные результаты и создаем ЭКО результаты при необходимости
            foreach (var outputItem in NormalOutputs)
            {
                if (!TryBuildOutput(outputItem, out var output))
                {
                    await DisplayAlert("Ошибка", "Заполните все нормальные результаты корректно", "OK");
                    return;
                }
                output.OutputType = ReprocessingOutputType.Normal;
                outputs.Add(output);

                // Если указано количество ЭКО, создаем ЭКО результат с тем же продуктом/материалом
                if (int.TryParse(outputItem.EcoQuantity, out var ecoQty) && ecoQty > 0)
                {
                    var ecoOutput = new ReprocessingOutput
                    {
                        OutputType = ReprocessingOutputType.Eco,
                        Quantity = ecoQty,
                        MeasuringType = output.MeasuringType,
                        MaterialId = output.MaterialId,
                        ProductId = output.ProductId,
                        NewMaterialCode = output.NewMaterialCode,
                        NewMaterialName = output.NewMaterialName,
                        NewMaterialDescription = output.NewMaterialDescription
                    };
                    outputs.Add(ecoOutput);
                }
            }

            // Получаем количество брака и переработки
            var defectQuantity = 0;
            var recyclingQuantity = 0;
            int? defectWarehouseId = null;
            int? recyclingWarehouseId = null;

            if (int.TryParse(DefectQuantityEntry?.Text, out var parsedDefect) && parsedDefect > 0)
            {
                defectQuantity = parsedDefect;
                if (_selectedDefectWarehouse != null)
                {
                    defectWarehouseId = _selectedDefectWarehouse.Id;
                }
                else if (_disposalWarehouses.Count == 1)
                {
                    // Если только один склад утиля, используем его
                    defectWarehouseId = _disposalWarehouses[0].Id;
                }
                else if (_disposalWarehouses.Count > 1)
                {
                    await DisplayAlert("Ошибка", "Выберите склад утиля для невозвратного брака", "OK");
                    return;
                }
                else if (_disposalWarehouses.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Склад утиля не найден. Создайте склад с типом 'Утиль'", "OK");
                    return;
                }
            }

            if (int.TryParse(RecyclingQuantityEntry?.Text, out var parsedRecycling) && parsedRecycling > 0)
            {
                recyclingQuantity = parsedRecycling;
                if (_selectedRecyclingWarehouse != null)
                {
                    recyclingWarehouseId = _selectedRecyclingWarehouse.Id;
                }
                else if (_disposalWarehouses.Count == 1)
                {
                    // Если только один склад утиля, используем его
                    recyclingWarehouseId = _disposalWarehouses[0].Id;
                }
                else if (_disposalWarehouses.Count > 1)
                {
                    await DisplayAlert("Ошибка", "Выберите склад утиля для переработки", "OK");
                    return;
                }
                else if (_disposalWarehouses.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Склад утиля не найден. Создайте склад с типом 'Утиль'", "OK");
                    return;
                }
            }

            var request = new ReprocessingCreateRequest
            {
                WarehouseId = warehouse.Id,
                Sources = sources,
                Outputs = outputs,
                DefectQuantity = defectQuantity,
                DefectWarehouseId = defectWarehouseId,
                RecyclingQuantity = recyclingQuantity,
                RecyclingWarehouseId = recyclingWarehouseId,
                Note = NoteEditor?.Text?.Trim()
            };

            var response = await _apiService.CreateReprocessingAsync(request);
            if (response.GetData() == null)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Не удалось выполнить переработку", "OK");
                return;
            }

            await DisplayAlert("Успех", "Производство выполнено", "OK");
            ClearForm();
        }

        private static bool TryBuildOutput(OutputItem outputItem, out ReprocessingOutput output)
        {
            output = new ReprocessingOutput();

            if (!int.TryParse(outputItem.Quantity, out var quantity) || quantity <= 0)
            {
                return false;
            }

            if (outputItem.SelectedType == "Материал")
            {
                // Для материала требуется код и название
                if (string.IsNullOrWhiteSpace(outputItem.MaterialCode) || 
                    string.IsNullOrWhiteSpace(outputItem.MaterialName))
                {
                    return false;
                }
                
                // Новый материал
                output.MaterialId = null;
                output.NewMaterialCode = outputItem.MaterialCode;
                output.NewMaterialName = outputItem.MaterialName;
                output.Quantity = quantity;
                output.MeasuringType = outputItem.MaterialMeasuringUnit ?? "шт";
                return true;
            }

            if (outputItem.SelectedType == "Продукт" && outputItem.SelectedProduct != null)
            {
                output.MaterialId = null;
                output.NewMaterialCode = null;
                output.NewMaterialName = null;
                output.ProductId = outputItem.SelectedProduct.Id;
                output.Quantity = quantity;
                output.MeasuringType = outputItem.SelectedProduct.MeasuringUnit;
                return true;
            }

            return false;
        }

        private void ClearForm()
        {
            Sources.Clear();
            var availableMaterials = GetAvailableMaterials();
            Sources.Add(new SourceItem(availableMaterials, IsDesktop()));
            Outputs.Clear();
            Outputs.Add(new OutputItem(_products));
            WarehousePicker.SelectedItem = null;
            _selectedWarehouse = null;
            DefectQuantityEntry.Text = "0";
        }

        private static bool TryBuildSource(SourceItem sourceItem, out ReprocessingSource source)
        {
            source = new ReprocessingSource();

            if (sourceItem.SelectedMaterial is not Material material)
            {
                return false;
            }

            if (!int.TryParse(sourceItem.Quantity, out var quantity) || quantity <= 0)
            {
                return false;
            }

            source.MaterialId = material.Id;
            source.Quantity = quantity;
            source.MeasuringType = material.MeasuringUnit;
            return true;
        }

        public class SourceItem : INotifyPropertyChanged
        {
            private readonly List<Material> _availableMaterials;
            private readonly bool _isDesktop;
            private Material? _selectedMaterial;
            private string? _quantity;
            private IList<Material> _materialItems = new List<Material>();

            public SourceItem(List<Material> availableMaterials, bool isDesktop)
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
                    {
                        return;
                    }

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
                    {
                        return;
                    }

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

        public class OutputItem : INotifyPropertyChanged
        {
            private List<Product> _allProducts;
            private readonly bool _isDesktop;
            private string? _selectedType;
            private string? _quantity;
            private ReprocessingOutputType _outputType = ReprocessingOutputType.Normal;
            
            // Для материала
            private string? _materialCode;
            private string? _materialName;
            private string? _materialMeasuringUnit;
            
            // Для продукта
            private string? _productSearchText;
            private Product? _selectedProduct;
            private List<Product> _filteredProducts = new();
            
            // Для ЭКО продукции
            private string? _ecoQuantity;

            public OutputItem(List<Product> products)
            {
                _allProducts = products.Where(p => p.IsActive).ToList();
                _filteredProducts = _allProducts.ToList();
                TypeOptions = new List<string> { "Материал", "Продукт" };
                MeasuringUnitOptions = new List<string> { "шт", "кг", "г", "л", "мл", "м", "см" };
                _materialMeasuringUnit = "шт";
                _isDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Platform == DevicePlatform.WinUI;
            }

            public double PickerHeight => _isDesktop ? 58.0 : 48.0;

            public List<string> TypeOptions { get; }
            public List<string> MeasuringUnitOptions { get; }

            public string? SelectedType
            {
                get => _selectedType;
                set
                {
                    if (_selectedType == value) return;
                    _selectedType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMaterialSelected));
                    OnPropertyChanged(nameof(IsProductSelected));
                    OnPropertyChanged(nameof(MeasuringUnit));
                    
                    // Сбрасываем данные при смене типа
                    MaterialCode = null;
                    MaterialName = null;
                    SelectedProduct = null;
                    ProductSearchText = null;
                    OnPropertyChanged(nameof(OutputDisplayName));
                }
            }

            public bool IsMaterialSelected => _selectedType == "Материал";
            public bool IsProductSelected => _selectedType == "Продукт";

            // === Материал ===
            public string? MaterialCode
            {
                get => _materialCode;
                set
                {
                    if (_materialCode == value) return;
                    _materialCode = value;
                    OnPropertyChanged();
                }
            }

            public string? MaterialName
            {
                get => _materialName;
                set
                {
                    if (_materialName == value) return;
                    _materialName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MeasuringUnit));
                    OnPropertyChanged(nameof(OutputDisplayName));
                }
            }

            public string? MaterialMeasuringUnit
            {
                get => _materialMeasuringUnit;
                set
                {
                    if (_materialMeasuringUnit == value) return;
                    _materialMeasuringUnit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MeasuringUnit));
                }
            }

            // === Продукт ===
            public string? ProductSearchText
            {
                get => _productSearchText;
                set
                {
                    if (_productSearchText == value) return;
                    _productSearchText = value;
                    OnPropertyChanged();
                    FilterProducts();
                }
            }

            public List<Product> FilteredProducts
            {
                get => _filteredProducts;
                private set
                {
                    _filteredProducts = value;
                    OnPropertyChanged();
                }
            }

            public Product? SelectedProduct
            {
                get => _selectedProduct;
                set
                {
                    if (_selectedProduct == value) return;
                    _selectedProduct = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MeasuringUnit));
                    OnPropertyChanged(nameof(HasSelectedProduct));
                    OnPropertyChanged(nameof(SelectedProductInfo));
                    OnPropertyChanged(nameof(OutputDisplayName));
                }
            }

            public bool HasSelectedProduct => _selectedProduct != null;

            public string SelectedProductInfo => _selectedProduct != null 
                ? $"Артикул: {_selectedProduct.Code ?? "—"} | Ед. изм.: {_selectedProduct.MeasuringUnit}"
                : "";

            // === Общее ===
            public string MeasuringUnit
            {
                get
                {
                    if (IsMaterialSelected)
                    {
                        return _materialMeasuringUnit ?? "шт";
                    }
                    if (IsProductSelected && _selectedProduct != null)
                    {
                        return _selectedProduct.MeasuringUnit;
                    }
                    return "";
                }
            }

            public string? Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity == value) return;
                    _quantity = value;
                    OnPropertyChanged();
                }
            }

            public ReprocessingOutputType OutputType
            {
                get => _outputType;
                set
                {
                    if (_outputType == value) return;
                    _outputType = value;
                    OnPropertyChanged();
                }
            }

            public string? EcoQuantity
            {
                get => _ecoQuantity;
                set
                {
                    if (_ecoQuantity == value) return;
                    _ecoQuantity = value;
                    OnPropertyChanged();
                }
            }

            public string OutputDisplayName
            {
                get
                {
                    if (IsMaterialSelected && !string.IsNullOrWhiteSpace(_materialName))
                    {
                        return $"Материал: {_materialName}";
                    }
                    if (IsProductSelected && _selectedProduct != null)
                    {
                        return $"Продукт: {_selectedProduct.Name}";
                    }
                    return "Результат";
                }
            }

            public void UpdateProducts(List<Product> products)
            {
                _allProducts = products.Where(p => p.IsActive).ToList();
                FilterProducts();
            }

            private void FilterProducts()
            {
                List<Product> newList;
                if (string.IsNullOrWhiteSpace(_productSearchText))
                {
                    newList = _allProducts.ToList();
                }
                else
                {
                    var search = _productSearchText.Trim().ToLowerInvariant();
                    newList = _allProducts
                        .Where(p => (p.Name?.ToLowerInvariant().Contains(search) ?? false) ||
                                    (p.Code?.ToLowerInvariant().Contains(search) ?? false))
                        .ToList();
                }

                // Если выбранный продукт не входит в новый список — сбрасываем выбор,
                // иначе Picker падает (SelectedItem не из текущего ItemsSource)
                if (_selectedProduct != null && !newList.Any(p => p.Id == _selectedProduct.Id))
                {
                    _selectedProduct = null;
                    OnPropertyChanged(nameof(SelectedProduct));
                    OnPropertyChanged(nameof(HasSelectedProduct));
                    OnPropertyChanged(nameof(SelectedProductInfo));
                    OnPropertyChanged(nameof(MeasuringUnit));
                }

                FilteredProducts = newList;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

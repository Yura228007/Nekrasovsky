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
        private Warehouse? _selectedWarehouse;

        public ObservableCollection<SourceItem> Sources { get; } = new();
        public ObservableCollection<OutputItem> Outputs { get; } = new();
        public ICommand AddSourceCommand { get; }
        public ICommand RemoveSourceCommand { get; }
        public ICommand AddOutputCommand { get; }
        public ICommand RemoveOutputCommand { get; }

        public ReprocessingPage(IApiService apiService, IAuthService authService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            AddSourceCommand = new Command(AddSource);
            RemoveSourceCommand = new Command<SourceItem>(RemoveSource);
            AddOutputCommand = new Command(AddOutput);
            RemoveOutputCommand = new Command<OutputItem>(RemoveOutput);
            BindingContext = this;
            // Инициализируем с пустым списком, данные загрузятся в OnAppearing
            Sources.Add(new SourceItem(new List<Material>(), IsDesktop()));
            Outputs.Add(new OutputItem(_materials, _products));
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
            if (IsDesktop() && WarehousePickerBorder != null)
            {
                WarehousePickerBorder.HeightRequest = 58.0;
            }
            else if (WarehousePickerBorder != null)
            {
                WarehousePickerBorder.HeightRequest = 48.0;
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

            _warehouses.AddRange((await _apiService.GetAllWarehousesAsync())
                .Where(w => w.IsActive));
            _materials.AddRange(await _apiService.GetAllMaterialsAsync());
            _products.AddRange(await _apiService.GetAllProductsAsync());
            _fillingWarehouses.AddRange(await _apiService.GetAllFillingWarehousesAsync());

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
            
            // Обновляем материалы для исходников после загрузки данных
            UpdateSourceMaterials();
            
            foreach (var output in Outputs)
            {
                output.RefreshTargets();
            }
        }

        private void OnWarehouseSelected(object? sender, EventArgs e)
        {
            _selectedWarehouse = WarehousePicker.SelectedItem as Warehouse;
            UpdateSourceMaterials();
        }

        private void UpdateSourceMaterials()
        {
            // Получаем материалы, которые есть на выбранном складе И под ответственностью пользователя
            var availableMaterials = _responsibleMaterials.AsEnumerable();

            if (_selectedWarehouse != null)
            {
                // Получаем ID материалов, которые есть на выбранном складе
                var materialIdsOnWarehouse = _fillingWarehouses
                    .Where(fw => fw.WarehouseId == _selectedWarehouse.Id && 
                                 fw.MaterialId.HasValue && 
                                 fw.Quantity > 0)
                    .Select(fw => fw.MaterialId!.Value)
                    .ToHashSet();

                // Фильтруем материалы: должны быть и под ответственностью, и на складе
                availableMaterials = availableMaterials.Where(m => materialIdsOnWarehouse.Contains(m.Id));
            }

            var materialsList = availableMaterials.ToList();
            
            // Обновляем материалы для всех SourceItem
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
            if (_selectedWarehouse != null)
            {
                var materialIdsOnWarehouse = _fillingWarehouses
                    .Where(fw => fw.WarehouseId == _selectedWarehouse.Id && 
                                 fw.MaterialId.HasValue && 
                                 fw.Quantity > 0)
                    .Select(fw => fw.MaterialId!.Value)
                    .ToHashSet();

                return _responsibleMaterials.Where(m => materialIdsOnWarehouse.Contains(m.Id)).ToList();
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
            Outputs.Add(new OutputItem(_materials, _products));
        }

        private void RemoveOutput(OutputItem? item)
        {
            if (item == null || Outputs.Count <= 1)
            {
                return;
            }

            Outputs.Remove(item);
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

            foreach (var outputItem in Outputs)
            {
                if (!TryBuildOutput(outputItem, out var output))
                {
                    await DisplayAlert("Ошибка", "Заполните все результаты корректно", "OK");
                    return;
                }

                outputs.Add(output);
            }

            var request = new ReprocessingCreateRequest
            {
                WarehouseId = warehouse.Id,
                Sources = sources,
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

        private static bool TryBuildOutput(OutputItem outputItem, out ReprocessingOutput output)
        {
            output = new ReprocessingOutput();

            if (!int.TryParse(outputItem.Quantity, out var quantity) || quantity <= 0)
            {
                return false;
            }

            if (outputItem.SelectedType == "Материал" && outputItem.SelectedTarget is Material material)
            {
                output.MaterialId = material.Id;
                output.Quantity = quantity;
                output.MeasuringType = material.MeasuringUnit;
                return true;
            }

            if (outputItem.SelectedType == "Продукт" && outputItem.SelectedTarget is Product product)
            {
                output.ProductId = product.Id;
                output.Quantity = quantity;
                output.MeasuringType = product.MeasuringUnit;
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
            Outputs.Add(new OutputItem(_materials, _products));
            WarehousePicker.SelectedItem = null;
            _selectedWarehouse = null;
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
                }
            }

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
            private readonly List<Material> _materials;
            private readonly List<Product> _products;
            private readonly bool _isDesktop;
            private string? _selectedType;
            private object? _selectedTarget;
            private string? _quantity;
            private IList<object> _targetItems = new List<object>();

            public OutputItem(List<Material> materials, List<Product> products)
            {
                _materials = materials;
                _products = products;
                TypeOptions = new List<string> { "Материал", "Продукт" };
                _isDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Platform == DevicePlatform.WinUI;
            }

            public double PickerHeight => _isDesktop ? 58.0 : 48.0;

            public List<string> TypeOptions { get; }

            public string? SelectedType
            {
                get => _selectedType;
                set
                {
                    if (_selectedType == value)
                    {
                        return;
                    }

                    _selectedType = value;
                    OnPropertyChanged();
                    UpdateTargetItems();
                }
            }

            public IList<object> TargetItems
            {
                get => _targetItems;
                private set
                {
                    _targetItems = value;
                    OnPropertyChanged();
                }
            }

            public object? SelectedTarget
            {
                get => _selectedTarget;
                set
                {
                    if (_selectedTarget == value)
                    {
                        return;
                    }

                    _selectedTarget = value;
                    OnPropertyChanged();
                }
            }

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

            public void RefreshTargets()
            {
                UpdateTargetItems();
            }

            private void UpdateTargetItems()
            {
                if (_selectedType == "Материал")
                {
                    TargetItems = _materials.Cast<object>().ToList();
                    SelectedTarget = null;
                    return;
                }

                if (_selectedType == "Продукт")
                {
                    TargetItems = _products.Cast<object>().ToList();
                    SelectedTarget = null;
                    return;
                }

                TargetItems = new List<object>();
                SelectedTarget = null;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

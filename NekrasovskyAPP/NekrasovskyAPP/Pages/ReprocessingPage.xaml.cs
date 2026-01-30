using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;

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
            Sources.Add(new SourceItem(_responsibleMaterials));
            Outputs.Add(new OutputItem(_materials, _products));
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

            _warehouses.AddRange((await _apiService.GetAllWarehousesAsync())
                .Where(w => w.IsActive));
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
            foreach (var source in Sources)
            {
                source.RefreshMaterials();
            }
            foreach (var output in Outputs)
            {
                output.RefreshTargets();
            }
        }

        private void AddSource()
        {
            Sources.Add(new SourceItem(_responsibleMaterials));
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
            Sources.Add(new SourceItem(_responsibleMaterials));
            Outputs.Clear();
            Outputs.Add(new OutputItem(_materials, _products));
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
            private readonly List<Material> _materials;
            private Material? _selectedMaterial;
            private string? _quantity;
            private IList<Material> _materialItems = new List<Material>();

            public SourceItem(List<Material> materials)
            {
                _materials = materials;
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

            public void RefreshMaterials()
            {
                Materials = _materials.ToList();
                SelectedMaterial = null;
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
            private string? _selectedType;
            private object? _selectedTarget;
            private string? _quantity;
            private IList<object> _targetItems = new List<object>();

            public OutputItem(List<Material> materials, List<Product> products)
            {
                _materials = materials;
                _products = products;
                TypeOptions = new List<string> { "Материал", "Продукт" };
            }

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

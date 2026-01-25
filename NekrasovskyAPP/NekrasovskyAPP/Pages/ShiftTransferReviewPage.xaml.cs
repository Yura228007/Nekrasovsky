using System.Collections.ObjectModel;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class ShiftTransferReviewPage : ContentPage
    {
        private readonly TaskCompletionSource<bool> _tcs = new();

        public ObservableCollection<ResponsibilityStockItem> StockItems { get; }

        public bool AllChecked => StockItems.Count > 0 && StockItems.All(item => item.IsChecked);

        public Task<bool> ConfirmationTask => _tcs.Task;

        public ShiftTransferReviewPage(IEnumerable<ResponsibilityStockItem> items)
        {
            InitializeComponent();
            StockItems = new ObservableCollection<ResponsibilityStockItem>(items);
            BindingContext = this;
            UpdateConfirmState();
        }

        private void OnCheckChanged(object? sender, CheckedChangedEventArgs e)
        {
            UpdateConfirmState();
        }

        private void UpdateConfirmState()
        {
            OnPropertyChanged(nameof(AllChecked));
            ConfirmButton.IsEnabled = AllChecked;
        }

        private async void OnConfirmClicked(object? sender, EventArgs e)
        {
            _tcs.TrySetResult(true);
            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            _tcs.TrySetResult(false);
            await Navigation.PopModalAsync();
        }
    }
}

using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class WorkReportsPage : ContentPage
    {
        private readonly WorkReportsViewModel _viewModel;

        public WorkReportsPage(WorkReportsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWorkReportsAsync();
            UpdateStartWorkVisibility();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadWorkReportsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadWorkReportsAsync();
            UpdateStartWorkVisibility();
        }

        private async void OnStartWorkClicked(object? sender, EventArgs e)
        {
            await _viewModel.StartWorkAsync();
        }

        private async void OnFinishWorkClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is WorkReport report)
            {
                await _viewModel.FinishWorkAsync(report.Id);
            }
        }

        private void UpdateStartWorkVisibility()
        {
            if (_viewModel.CanManageShiftManually)
            {
                if (!ToolbarItems.Contains(StartWorkToolbarItem))
                {
                    ToolbarItems.Add(StartWorkToolbarItem);
                }
            }
            else
            {
                if (ToolbarItems.Contains(StartWorkToolbarItem))
                {
                    ToolbarItems.Remove(StartWorkToolbarItem);
                }
            }
        }
    }
}

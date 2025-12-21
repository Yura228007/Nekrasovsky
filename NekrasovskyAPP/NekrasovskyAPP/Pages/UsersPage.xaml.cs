using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

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
            await Shell.Current.GoToAsync("//UserDetailPage");
        }

        private async void OnUserSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is User selectedUser)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "User", selectedUser }
                };
                await Shell.Current.GoToAsync("//UserDetailPage", navigationParameter);
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
                
                // Search will be implemented in ViewModel
                await _viewModel.LoadUsersAsync();
            }
        }
    }
}


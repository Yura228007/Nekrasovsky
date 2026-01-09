using NekrasovskyAPP.Pages;

namespace NekrasovskyAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for pages that are NOT ShellContent
            // LoginPage, HomePage, and AdminPage are defined as ShellContent in XAML,
            // so they are automatically registered and should NOT be registered here
            
            Routing.RegisterRoute("UsersPage", typeof(UsersPage));
            Routing.RegisterRoute("ProductsPage", typeof(ProductsPage));
            Routing.RegisterRoute("MaterialsPage", typeof(MaterialsPage));
            Routing.RegisterRoute("WarehousesPage", typeof(WarehousesPage));
            Routing.RegisterRoute("WorkReportsPage", typeof(WorkReportsPage));
            Routing.RegisterRoute("PartRequestsPage", typeof(PartRequestsPage));
            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
        }
    }
}

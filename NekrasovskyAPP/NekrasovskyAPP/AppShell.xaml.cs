using NekrasovskyAPP.Pages;

namespace NekrasovskyAPP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("HomePage", typeof(HomePage));
            Routing.RegisterRoute("AdminPage", typeof(AdminPage));
            Routing.RegisterRoute("UsersPage", typeof(UsersPage));
            Routing.RegisterRoute("ProductsPage", typeof(ProductsPage));
            Routing.RegisterRoute("MaterialsPage", typeof(MaterialsPage));
            Routing.RegisterRoute("WarehousesPage", typeof(WarehousesPage));
            Routing.RegisterRoute("WorkReportsPage", typeof(WorkReportsPage));
            Routing.RegisterRoute("PartRequestsPage", typeof(PartRequestsPage));
        }
    }
}

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
            Routing.RegisterRoute("ShiftTransfersPage", typeof(ShiftTransfersPage));
            Routing.RegisterRoute("ReprocessingPage", typeof(ReprocessingPage));
            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
            Routing.RegisterRoute("HistoryPage", typeof(HistoryPage));
            Routing.RegisterRoute("RolePermissionsPage", typeof(RolePermissionsPage));
            Routing.RegisterRoute("DisposalPage", typeof(DisposalPage));
            Routing.RegisterRoute("QrCodesPage", typeof(QrCodesPage));
            Routing.RegisterRoute("ProductOutputPage", typeof(ProductOutputPage));
            Routing.RegisterRoute("MachinesPage", typeof(MachinesPage));
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NekrasovskyAPP.Services;
using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Pages;
using AppDynamics.Agent;
#if ANDROID || IOS
using BarcodeScanning;
#endif

namespace NekrasovskyAPP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            var mauiAppBuilder = builder
                .UseMauiApp<App>();
            
#if ANDROID || IOS
            // BarcodeScanning поддерживается только на Android и iOS
            mauiAppBuilder.UseBarcodeScanning();
#endif
            
            mauiAppBuilder.ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ISignalRService, SignalRService>();
            builder.Services.AddSingleton<IAlarmSoundService, AlarmSoundService>();
            builder.Services.AddSingleton<IAlarmNotificationService, AlarmNotificationService>();
            builder.Services.AddSingleton<MainViewModel>(sp =>
                new MainViewModel(
                    sp.GetRequiredService<IApiService>(),
                    sp.GetRequiredService<IAuthService>()));
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<ViewModels.WorkReportsViewModel>();
            builder.Services.AddSingleton<ViewModels.PartRequestsViewModel>(sp =>
                new ViewModels.PartRequestsViewModel(
                    sp.GetRequiredService<IApiService>(),
                    sp.GetRequiredService<IAuthService>()));
            builder.Services.AddSingleton<ViewModels.ShiftTransfersViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<AdminPage>();
            builder.Services.AddTransient<UsersPage>(sp => 
                new UsersPage(
                    sp.GetRequiredService<MainViewModel>(),
                    sp.GetRequiredService<IAuthService>()));
            builder.Services.AddTransient<ProductsPage>(sp => 
                new ProductsPage(sp.GetRequiredService<MainViewModel>()));
            builder.Services.AddTransient<MaterialsPage>(sp => 
                new MaterialsPage(sp.GetRequiredService<MainViewModel>()));
            builder.Services.AddTransient<WarehousesPage>(sp => 
                new WarehousesPage(sp.GetRequiredService<MainViewModel>()));
            builder.Services.AddTransient<WorkReportsPage>(sp => 
                new WorkReportsPage(sp.GetRequiredService<ViewModels.WorkReportsViewModel>()));
            builder.Services.AddTransient<PartRequestsPage>(sp => 
                new PartRequestsPage(sp.GetRequiredService<ViewModels.PartRequestsViewModel>()));
            builder.Services.AddTransient<ShiftTransfersPage>(sp =>
                new ShiftTransfersPage(sp.GetRequiredService<ViewModels.ShiftTransfersViewModel>()));
            builder.Services.AddTransient<ShiftTransferReviewPage>();
            builder.Services.AddTransient<ReprocessingPage>(sp =>
                new ReprocessingPage(sp.GetRequiredService<IApiService>(), sp.GetRequiredService<IAuthService>()));
            builder.Services.AddTransient<SettingsPage>(sp =>
                new SettingsPage(sp.GetRequiredService<IApiService>()));
            builder.Services.AddTransient<ViewModels.HistoryViewModel>();
            builder.Services.AddTransient<HistoryPage>(sp =>
                new HistoryPage(sp.GetRequiredService<ViewModels.HistoryViewModel>()));
            builder.Services.AddTransient<RolePermissionsPage>(sp =>
                new RolePermissionsPage(
                    sp.GetRequiredService<IApiService>(),
                    sp.GetRequiredService<IAuthService>()));
            builder.Services.AddTransient<ViewModels.DisposalViewModel>(sp =>
                new ViewModels.DisposalViewModel(
                    sp.GetRequiredService<IApiService>(),
                    sp.GetRequiredService<IAuthService>()));
            builder.Services.AddTransient<DisposalPage>(sp =>
                new DisposalPage(sp.GetRequiredService<ViewModels.DisposalViewModel>()));
#if ANDROID || IOS
            // BarcodeScannerPage доступна только на Android и iOS
            builder.Services.AddTransient<BarcodeScannerPage>();
#endif

            return builder.Build();
        }
    }
}

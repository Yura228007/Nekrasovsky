using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NekrasovskyAPP.Services;
using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Pages;

namespace NekrasovskyAPP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
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
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<ViewModels.WorkReportsViewModel>();
            builder.Services.AddSingleton<ViewModels.PartRequestsViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<AdminPage>();
            builder.Services.AddTransient<UsersPage>(sp => 
                new UsersPage(sp.GetRequiredService<MainViewModel>()));
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

            return builder.Build();
        }
    }
}

using Microsoft.Extensions.Logging;
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

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<AdminPage>();
            builder.Services.AddTransient<UsersPage>();
            builder.Services.AddTransient<ProductsPage>();
            builder.Services.AddTransient<MaterialsPage>();
            builder.Services.AddTransient<WarehousesPage>();
            builder.Services.AddTransient<WorkReportsPage>();
            builder.Services.AddTransient<PartRequestsPage>();

            return builder.Build();
        }
    }
}

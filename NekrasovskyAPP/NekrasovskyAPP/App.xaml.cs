namespace NekrasovskyAPP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            // Устанавливаем темную тему по умолчанию
            UserAppTheme = AppTheme.Dark;
            
            MainPage = new AppShell();
        }
    }
}

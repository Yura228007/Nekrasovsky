using System.Globalization;

namespace NekrasovskyAPP.Converters
{
    public class BoolToStatusTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Работает" : "Остановлен";
            }
            return "Неизвестно";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToStatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                // Используем цвета из ресурсов
                return isActive ? Color.FromArgb("#2ECC71") : Color.FromArgb("#E74C3C"); // Success : Error
            }
            return Color.FromArgb("#616161"); // Placeholder
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

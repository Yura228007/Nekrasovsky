using System.Globalization;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Converters
{
    public class IsPendingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PartRequestStatus status)
            {
                return status == PartRequestStatus.Pending;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}



using System.Globalization;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Converters
{
    public class IsPendingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PartRequestStatus partStatus)
                return partStatus == PartRequestStatus.Pending;
            if (value is ProductMovementStatus movementStatus)
                return movementStatus == ProductMovementStatus.Pending;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}



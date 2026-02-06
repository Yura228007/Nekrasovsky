using System.Globalization;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Converters
{
    public class FinishedGoodsRequestTypeToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FinishedGoodsRequestType requestType)
            {
                return requestType == FinishedGoodsRequestType.Normal
                    ? Application.Current?.Resources["Primary"] as Color
                    : Application.Current?.Resources["Success"] as Color;
            }
            return Application.Current?.Resources["Gray500"] as Color;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

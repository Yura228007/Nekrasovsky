using System.Globalization;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Converters;

public class DisposalRequestTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DisposalRequestType requestType)
        {
            return requestType == DisposalRequestType.Defect ? "Невозвратный брак" : "Производство";
        }
        return "Неизвестно";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

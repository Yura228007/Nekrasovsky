using System.Globalization;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Converters;

public class DisposalRequestTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DisposalRequestType requestType)
        {
            // Невозвратный брак - иконка мусорки
            // Производство - иконка переработки
            return requestType == DisposalRequestType.Defect ? "🗑️" : "♻️";
        }
        return "❓";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

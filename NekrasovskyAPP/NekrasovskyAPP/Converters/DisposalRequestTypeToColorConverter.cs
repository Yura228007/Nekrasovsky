using System.Globalization;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Converters;

public class DisposalRequestTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DisposalRequestType requestType)
        {
            // Невозвратный брак - красный цвет (Error)
            // Производство - оранжевый/желтый цвет (Warning)
            return requestType == DisposalRequestType.Defect 
                ? Application.Current?.Resources["Error"] 
                : Application.Current?.Resources["Warning"];
        }
        return Application.Current?.Resources["Gray400"];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

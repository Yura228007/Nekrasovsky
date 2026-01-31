using System.Globalization;

namespace NekrasovskyAPP.Converters
{
    public class WarehouseTypeToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string warehouseType)
            {
                return warehouseType.ToLowerInvariant() switch
                {
                    "цех" => "🔧",
                    "склад" => "🏭",
                    "производство" => "⚙️",
                    "готовой продукции" or "готовая продукция" => "📦",
                    "сырья" or "сырьё" => "🔩",
                    "утиль" => "🗑️",
                    _ => "🏭"
                };
            }
            return "🏭";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

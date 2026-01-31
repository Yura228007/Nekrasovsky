using System.Globalization;
using Microsoft.Maui.Graphics;

namespace NekrasovskyAPP.Converters
{
    public class WarehouseTypeToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string warehouseType)
            {
                var type = warehouseType.ToLowerInvariant();
                return type switch
                {
                    "цех" => GetColorResource("CardMaterials"),
                    "склад" => GetColorResource("CardWarehouses"),
                    "производство" => GetColorResource("CardReprocess"),
                    "готовой продукции" or "готовая продукция" => GetColorResource("CardProducts"),
                    "сырья" or "сырьё" => GetColorResource("CardScanner"),
                    "утиль" => GetColorResource("CardDisposal"),
                    _ => GetColorResource("CardWarehouses")
                };
            }

            return GetColorResource("CardWarehouses");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Color GetColorResource(string key)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var resource) == true &&
                resource is Color color)
            {
                return color;
            }

            return Colors.Green;
        }
    }
}

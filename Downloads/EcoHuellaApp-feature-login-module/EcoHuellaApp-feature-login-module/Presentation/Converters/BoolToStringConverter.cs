using System.Globalization;

namespace EcoHuellaApp.Presentation.Converters
{
    public sealed class BoolToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool b || parameter is not string param) return value;
            var parts = param.Split('|');
            return parts.Length == 2 ? (b ? parts[0] : parts[1]) : value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

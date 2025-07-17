using System.Globalization;

namespace DnDClient.Converters;

public class BoolToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int intValue = (int)value;
        int checkIndex = int.Parse(parameter.ToString());
        return intValue >= checkIndex;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
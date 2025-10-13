using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GAG_Proc_Generator.Converters;

public class BoolToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBrush = new(
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2196F3")!);

    private static readonly SolidColorBrush InactiveBrush = new(
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#CCCCCC")!);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue && boolValue ? ActiveBrush : InactiveBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

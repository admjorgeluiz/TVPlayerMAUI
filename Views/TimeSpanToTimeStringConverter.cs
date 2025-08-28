using System.Globalization;

namespace TVPlayerMAUI.Views; // Ou TVPlayerMAUI.Converters

public class TimeSpanToTimeStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            // Se o vídeo tiver horas, mostra o formato h:mm:ss, senão, m:ss
            return ts.Hours > 0
                ? ts.ToString(@"h\:mm\:ss")
                : ts.ToString(@"m\:ss");
        }
        return "0:00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
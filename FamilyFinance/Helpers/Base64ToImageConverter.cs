using System.Globalization;

namespace FamilyFinance.Helpers;

public class Base64ToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string base64 && !string.IsNullOrWhiteSpace(base64))
        {
            try
            {
                var bytes = System.Convert.FromBase64String(base64);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                // Fall through to placeholder
            }
        }

        // Return a placeholder person icon
        return ImageSource.FromFile("person_placeholder.png");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

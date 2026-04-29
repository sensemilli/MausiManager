using System;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.JobManager
{
    /// <summary>
    /// Konvertiert TimeSpan zu Millisekunden (int) und zurück
    /// </summary>
    [ValueConversion(typeof(TimeSpan), typeof(int))]
    public class TimeSpanToMsConverter : IValueConverter
    {
        /// <summary>
        /// Konvertiert TimeSpan ? Millisekunden (int) für Anzeige im DataGrid
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return (int)timeSpan.TotalMilliseconds;
            }
            
            return 0;
        }

        /// <summary>
        /// Konvertiert Millisekunden (int/string) ? TimeSpan beim Editieren
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return TimeSpan.Zero;

            // Wenn der Benutzer einen String eingibt
            if (value is string stringValue)
            {
                if (int.TryParse(stringValue, out int ms))
                {
                    return TimeSpan.FromMilliseconds(ms);
                }
                return TimeSpan.Zero;
            }

            // Wenn direkt ein int-Wert übergeben wird
            if (value is int intValue)
            {
                return TimeSpan.FromMilliseconds(intValue);
            }

            // Wenn double übergeben wird (z.B. aus XAML)
            if (value is double doubleValue)
            {
                return TimeSpan.FromMilliseconds(doubleValue);
            }

            return TimeSpan.Zero;
        }
    }
}
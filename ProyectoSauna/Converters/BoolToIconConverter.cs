using System;
using System.Globalization;
using System.Windows.Data;

namespace ProyectoSauna.Converters
{
    /// <summary>
    /// Convierte un bool a un icono de Material Design
    /// </summary>
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return "CheckCircle"; // Icono para verdadero
            }
            return "MinusCircle"; // Icono para falso
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
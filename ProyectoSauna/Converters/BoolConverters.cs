using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProyectoSauna.Converters
{
    /// <summary>
    /// Convierte bool a SolidColorBrush (Verde si true, Rojo si false)
    /// Uso: Foreground="{Binding activo, Converter={StaticResource BoolToColorConverter}}"
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                // ✅ Devuelve SolidColorBrush, NO Colors
                return isActive
                    ? new SolidColorBrush(Colors.Green)
                    : new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                // ✅ Devuelve texto legible con símbolos
                return isActive ? "✓ Activo" : "✗ Inactivo";
            }
            return "? Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
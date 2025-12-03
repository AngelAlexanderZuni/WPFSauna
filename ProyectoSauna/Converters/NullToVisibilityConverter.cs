using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProyectoSauna.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No se soporta la conversi├│n inversa, retorna el valor original o Binding.DoNothing
            return Binding.DoNothing;
        }
    }
}

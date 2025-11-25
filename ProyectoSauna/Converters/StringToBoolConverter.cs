using System;
using System.Globalization;
using System.Windows.Data;

namespace ProyectoSauna.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
                return !string.IsNullOrWhiteSpace(str);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No se soporta la conversión inversa, retorna el valor original o Binding.DoNothing
            return Binding.DoNothing;
        }
    }
}

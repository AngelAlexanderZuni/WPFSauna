using System;
using System.Globalization;
using System.Windows;
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

    /// <summary>
    /// Convierte bool a Visibility 
    /// Parámetro "Inverse" invierte la lógica (false = Visible)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Si el parámetro es "Inverse", invertir la lógica
                bool shouldBeVisible = (parameter?.ToString()?.ToLower() == "inverse") ? !boolValue : boolValue;
                
                return shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isVisible = visibility == Visibility.Visible;
                // Si el parámetro es "Inverse", invertir la lógica
                return (parameter?.ToString()?.ToLower() == "inverse") ? !isVisible : isVisible;
            }
            return false;
        }
    }
}
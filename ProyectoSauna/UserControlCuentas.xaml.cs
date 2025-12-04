using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProyectoSauna
{
    public partial class UserControlCuentas : UserControl
    {
        private bool _devolucionExpandido = false;

        public UserControlCuentas()
        {
            InitializeComponent();
        }

        private void TextBox_NumerosSoloPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // ✅ MÉTODO QUE FALTABA - Expandir/Contraer panel de Devolución
        private void BtnToggleDevolucion_Click(object sender, RoutedEventArgs e)
        {
            if (_devolucionExpandido)
            {
                // Contraer
                Storyboard collapseStoryboard = (Storyboard)this.Resources["CollapseDevolucion"];
                collapseStoryboard.Begin();

                // Rotar flecha hacia abajo
                RotateTransform rotateTransform = (RotateTransform)IconDevolucionArrow.RenderTransform;
                DoubleAnimation rotateAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

                _devolucionExpandido = false;
            }
            else
            {
                // Expandir
                Storyboard expandStoryboard = (Storyboard)this.Resources["ExpandDevolucion"];
                expandStoryboard.Begin();

                // Rotar flecha hacia arriba
                RotateTransform rotateTransform = (RotateTransform)IconDevolucionArrow.RenderTransform;
                DoubleAnimation rotateAnimation = new DoubleAnimation
                {
                    To = 180,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

                _devolucionExpandido = true;
            }
        }
    }
}
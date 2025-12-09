using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ProyectoSauna.ViewModels;

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
            // 📝 LA LÓGICA ESTÁ AHORA EN SelectedDataGridItem PROPERTY
            System.Diagnostics.Debug.WriteLine($"🔄 DataGrid_SelectionChanged disparado - AddedItems: {e.AddedItems.Count}");
        }

        // 🚫 DETECTAR CLIC EN FILA DESHABILITADA
        private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && e.OriginalSource is FrameworkElement element)
            {
                var row = element.FindParent<DataGridRow>();
                
                if (row?.DataContext is CuentaPendiente cuenta && !cuenta.IsRadioButtonEnabled)
                {
                    var mensaje = $"⚠️ La cuenta '{cuenta.NombreCliente}' está siendo editada por {cuenta.UsuarioEditor}.\n\n" +
                                  $"• DNI: {cuenta.DocumentoCliente}\n" +
                                  $"• ID Cuenta: #{cuenta.idCuenta}\n\n" +
                                  "No puedes seleccionar esta cuenta mientras esté en edición.";
                    
                    MessageBox.Show(mensaje, "🔒 Cuenta en Edición", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Evitar que se procese la selección
                    e.Handled = true;
                    
                    // Resetear selección si se intentó seleccionar una fila bloqueada
                    if (dataGrid.DataContext is CuentasViewModel viewModel)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            dataGrid.SelectedItem = viewModel.SelectedDataGridItem;
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
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

    // 🔧 EXTENSIÓN HELPER PARA BUSCAR PADRE EN EL VISUAL TREE
    public static class VisualTreeExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            return parent is T ? parent as T : parent.FindParent<T>();
        }
    }
}
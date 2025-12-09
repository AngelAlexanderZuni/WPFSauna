using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using ProyectoSauna.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProyectoSauna
{
    /// <summary>
    /// Lógica de interacción para UserControlClientes.xaml
    /// </summary>
    public partial class UserControlClientes : UserControl{

        private ClientesViewModel ViewModel => DataContext as ClientesViewModel;

        private void Buscador_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (ViewModel == null) return;
            var tipo = ViewModel.TipoBusqueda?.ToLower();
            if (tipo == "dni")
            {
                // Solo permitir números
                e.Handled = !e.Text.All(char.IsDigit);
            }
            else if (tipo == "nombre")
            {
                // Solo permitir letras y espacios
                e.Handled = !e.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
            }
        }

        private void btnLimpiar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel?.LimpiarFormulario();
        }
    
        public UserControlClientes()
        {
            InitializeComponent();
            
            // Crear contexto de base de datos
            var context = new SaunaDbContext();
            
            // Crear repositorio
            var clienteRepository = new ClienteRepository(context);
            
            // Crear servicio
            var clienteService = new ClienteService(clienteRepository);
            
            // Crear ViewModel y asignarlo como DataContext
            var viewModel = new ClientesViewModel(clienteService);
            DataContext = viewModel;
        }

        // 🚫 DETECTAR CLIC EN FILA DESHABILITADA DE CLIENTES
        private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && e.OriginalSource is FrameworkElement element)
            {
                var row = FindParentRow(element);
                
                if (row?.DataContext is ClienteDTO cliente && !cliente.IsRadioButtonEnabled)
                {
                    var mensaje = $"⚠️ El cliente '{cliente.nombre} {cliente.apellidos}' está siendo editado por otro usuario.\n\n" +
                                  $"• DNI: {cliente.numero_documento}\n" +
                                  $"• ID Cliente: #{cliente.idCliente}\n\n" +
                                  "No puedes seleccionar este cliente mientras esté en edición.";
                    
                    MessageBox.Show(mensaje, "🔒 Cliente en Edición", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Evitar que se procese la selección
                    e.Handled = true;
                    
                    // Resetear selección si se intentó seleccionar una fila bloqueada
                    if (dataGrid.DataContext is ClientesViewModel viewModel)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            dataGrid.SelectedItem = viewModel.ClienteSeleccionado;
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
        }

        // 🔧 HELPER PARA BUSCAR FILA PADRE
        private DataGridRow FindParentRow(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is DataGridRow))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as DataGridRow;
        }
    }
}

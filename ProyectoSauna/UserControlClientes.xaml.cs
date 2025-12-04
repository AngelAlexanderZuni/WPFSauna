using ProyectoSauna.Models;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using ProyectoSauna.ViewModels;
using System.Windows.Controls;

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
            DataContext = new ClientesViewModel(clienteService);
        }
    }
}

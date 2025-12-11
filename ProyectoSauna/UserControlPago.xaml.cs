using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.ViewModels;

namespace ProyectoSauna
{
    /// <summary>
    /// UserControl para Pagos
    /// </summary>
    public partial class UserControlPago : UserControl
    {
        private readonly IServiceScope _scope;

        public UserControlPago()
        {
            InitializeComponent();
            
            // Create a scope for this view instance to resolve Scoped services like PagoService
            _scope = App.AppHost!.Services.CreateScope();
            
            // Get ViewModel from the scope
            this.DataContext = _scope.ServiceProvider.GetRequiredService<PagosViewModel>();
            
            this.Loaded += UserControlPago_Loaded;
            this.Unloaded += UserControlPago_Unloaded;
        }

        private void UserControlPago_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up resources when navigating away
            _scope.Dispose();
        }

        private async void UserControlPago_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PagosViewModel vm)
            {
                if (Application.Current.Properties.Contains("IdCuenta"))
                {
                    var idObj = Application.Current.Properties["IdCuenta"];
                    if (idObj != null && int.TryParse(idObj.ToString(), out int idCuenta))
                    {
                        await vm.CargarDatosAsync(idCuenta);
                    }
                }
            }
        }
    }
}